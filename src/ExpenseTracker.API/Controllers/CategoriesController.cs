using ExpenseTracker.Application.DTOs.Categories;
using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController(
    ICategoryService categoryService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await categoryService.GetAllAsync(
            currentUserService.UserId.Value,
            cancellationToken);

        return Ok(
            ApiResponseDto<IReadOnlyList<CategoryDto>>.Ok(
                result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await categoryService.CreateAsync(
            currentUserService.UserId.Value,
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return Conflict(
                ApiResponseDto<CategoryDto>.Fail(
                    "Unable to create category."));
        }

        return Created(
            $"/api/categories/{result.Data!.Id}",
            ApiResponseDto<CategoryDto>.Ok(
                result.Data));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCategoryDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await categoryService.UpdateAsync(
            currentUserService.UserId.Value,
            id,
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Category not found.")
            {
                return NotFound(
                    ApiResponseDto<CategoryDto>.Fail(
                        "Category not found."));
            }

            if (result.Error == "Access denied.")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<CategoryDto>.Fail(
                        "Access denied."));
            }

            return Conflict(
                ApiResponseDto<CategoryDto>.Fail(
                    "Unable to update category."));
        }

        return Ok(
            ApiResponseDto<CategoryDto>.Ok(
                result.Data!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await categoryService.DeleteAsync(
            currentUserService.UserId.Value,
            id,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Category not found.")
            {
                return NotFound(
                    ApiResponseDto<object>.Fail(
                        "Category not found."));
            }

            if (result.Error == "Access denied.")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<object>.Fail(
                        "Access denied."));
            }

            return Conflict(
                ApiResponseDto<object>.Fail(
                    "Unable to delete category."));
        }

        return NoContent();
    }
}