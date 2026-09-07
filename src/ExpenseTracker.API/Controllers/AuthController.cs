using System.Net.Http.Headers;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController(
    IAuthService authService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(
            request,
            cancellationToken);

        if (!result.IsSuccess)
            return Conflict(
                new { message = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(
            request,
            cancellationToken);

        if (!result.IsSuccess)
            return Unauthorized(
                new { message = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(
            request,
            cancellationToken);

        if (!result.IsSuccess)
            return Unauthorized(
                new { message = result.Error });

        return Ok(result.Data);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var accessToken =
            GetAccessTokenFromHeader();

        var result = await authService.LogoutAsync(
            currentUserService.UserId.Value,
            request.RefreshToken,
            accessToken,
            cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(
                new { message = result.Error });

        return NoContent();
    }

    private string? GetAccessTokenFromHeader()
    {
        var authorization =
            Request.Headers.Authorization.ToString();

        if (!AuthenticationHeaderValue.TryParse(
                authorization,
                out var header))
        {
            return null;
        }

        if (!string.Equals(
                header.Scheme,
                "Bearer",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return header.Parameter;
    }
}