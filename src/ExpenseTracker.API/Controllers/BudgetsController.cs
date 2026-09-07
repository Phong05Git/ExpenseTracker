using ExpenseTracker.Application.DTOs.Budgets;
using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController(
    IBudgetService budgetService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await budgetService.GetAllAsync(
            currentUserService.UserId.Value,
            cancellationToken);

        return Ok(
            ApiResponseDto<IReadOnlyList<BudgetDto>>.Ok(
                result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBudgetDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await budgetService.CreateAsync(
            currentUserService.UserId.Value,
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Category not found or access denied.")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<BudgetDto>.Fail(result.Error));
            }

            return Conflict(
                ApiResponseDto<BudgetDto>.Fail(result.Error!));
        }

        return Created(
            $"/api/budgets/{result.Data!.Id}",
            ApiResponseDto<BudgetDto>.Ok(result.Data));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateBudgetDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await budgetService.UpdateAsync(
            currentUserService.UserId.Value,
            id,
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Budget not found.")
            {
                return NotFound(
                    ApiResponseDto<BudgetDto>.Fail(result.Error));
            }

            if (result.Error == "Access denied.")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<BudgetDto>.Fail(result.Error));
            }

            if (result.Error == "Budget already exists for this category and period.")
            {
                return Conflict(
                    ApiResponseDto<BudgetDto>.Fail(result.Error));
            }

            return BadRequest(
                ApiResponseDto<BudgetDto>.Fail(result.Error!));
        }

        return Ok(
            ApiResponseDto<BudgetDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await budgetService.DeleteAsync(
            currentUserService.UserId.Value,
            id,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Budget not found.")
            {
                return NotFound(
                    ApiResponseDto<object>.Fail(result.Error));
            }

            if (result.Error == "Access denied.")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<object>.Fail(result.Error));
            }

            return BadRequest(
                ApiResponseDto<object>.Fail(result.Error!));
        }

        return NoContent();
    }
}