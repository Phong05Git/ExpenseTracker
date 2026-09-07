using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController(
    ITransactionService transactionService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] TransactionFilterDto filter,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await transactionService.FilterAsync(
            currentUserService.UserId.Value,
            filter,
            cancellationToken);

        return Ok(
            ApiResponseDto<PagedResultDto<TransactionDto>>.Ok(
                result.Data!));
    }

    [HttpGet("by-date")]
    public async Task<IActionResult> GetByDate(
        [FromQuery] DateOnly date,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await transactionService.GetByDateAsync(
            currentUserService.UserId.Value,
            date,
            cancellationToken);

        return Ok(
            ApiResponseDto<IReadOnlyList<TransactionDto>>.Ok(
                result.Data!));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await transactionService.CreateAsync(
            currentUserService.UserId.Value,
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Category not found or access denied.")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<TransactionDto>.Fail(result.Error));
            }

            return BadRequest(
                ApiResponseDto<TransactionDto>.Fail(result.Error!));
        }

        return Created(
            $"/api/transactions/{result.Data!.Id}",
            ApiResponseDto<TransactionDto>.Ok(result.Data));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateTransactionDto request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await transactionService.UpdateAsync(
            currentUserService.UserId.Value,
            id,
            request,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Transaction not found.")
            {
                return NotFound(
                    ApiResponseDto<TransactionDto>.Fail(
                        result.Error));
            }

            if (result.Error == "Access denied.")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<TransactionDto>.Fail(
                        result.Error));
            }

            if (result.Error == "Category not found or access denied.")
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<TransactionDto>.Fail(
                        result.Error));
            }

            return BadRequest(
                ApiResponseDto<TransactionDto>.Fail(
                    result.Error!));
        }

        return Ok(
            ApiResponseDto<TransactionDto>.Ok(result.Data!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var result = await transactionService.DeleteAsync(
            currentUserService.UserId.Value,
            id,
            cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error == "Transaction not found.")
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