using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.DTOs.Users;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(
    IUserService userService,
    IAuthService authService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await userService.GetProfileAsync(
            currentUserService.UserId.Value,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(
                ApiResponseDto<UserProfileDto>.Fail(
                    "User profile not found."));
        }

        return Ok(
            ApiResponseDto<UserProfileDto>.Ok(
                result.Data!));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await userService.UpdateProfileAsync(
            currentUserService.UserId.Value,
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "User not found.")
            {
                return NotFound(
                    ApiResponseDto<UserProfileDto>.Fail(
                        "User profile not found."));
            }

            return Conflict(
                ApiResponseDto<UserProfileDto>.Fail(
                    "Unable to update profile."));
        }

        return Ok(
            ApiResponseDto<UserProfileDto>.Ok(
                result.Data!));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var accessToken =
            Request.Headers.Authorization.ToString()
                .Replace(
                    "Bearer ",
                    string.Empty,
                    StringComparison.OrdinalIgnoreCase)
                .Trim();

        var result = await authService.ChangePasswordAsync(
            currentUserService.UserId.Value,
            request,
            accessToken,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "User not found.")
            {
                return NotFound(
                    ApiResponseDto<object>.Fail(
                        "User profile not found."));
            }

            return BadRequest(
                ApiResponseDto<object>.Fail(
                    "Unable to change password."));
        }

        return NoContent();
    }
}