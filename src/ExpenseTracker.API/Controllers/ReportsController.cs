using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(
    IReportService reportService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(
        [FromQuery] DateTimeOffset? referenceDate,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var date = referenceDate ?? DateTimeOffset.UtcNow;

        var result = await reportService.GetOverviewAsync(
            currentUserService.UserId.Value,
            date,
            cancellationToken);

        return Ok(
            ApiResponseDto<OverviewSummaryDto>.Ok(
                result.Data!));
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] PeriodType period,
        [FromQuery] DateTimeOffset? referenceDate,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        if (startDate.HasValue != endDate.HasValue)
        {
            return BadRequest(
                ApiResponseDto<StatisticsDto>.Fail(
                    "Khoảng thời gian không hợp lệ."));
        }

        if (startDate.HasValue &&
            startDate.Value.UtcDateTime.Date >
            endDate!.Value.UtcDateTime.Date)
        {
            return BadRequest(
                ApiResponseDto<StatisticsDto>.Fail(
                    "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc."));
        }

        var date = referenceDate ?? DateTimeOffset.UtcNow;

        var result = await reportService.GetStatisticsAsync(
            currentUserService.UserId.Value,
            period,
            date,
            startDate,
            endDate,
            cancellationToken);

        return Ok(
            ApiResponseDto<StatisticsDto>.Ok(
                result.Data!));
    }
}