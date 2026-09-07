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
        CancellationToken cancellationToken)
    {
        if (!currentUserService.UserId.HasValue)
            return Unauthorized();

        var date = referenceDate ?? DateTimeOffset.UtcNow;

        var result = await reportService.GetStatisticsAsync(
            currentUserService.UserId.Value,
            period,
            date,
            cancellationToken);

        return Ok(
            ApiResponseDto<StatisticsDto>.Ok(
                result.Data!));
    }
}