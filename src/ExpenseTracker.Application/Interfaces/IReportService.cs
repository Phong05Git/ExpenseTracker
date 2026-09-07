using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.Interfaces;

public interface IReportService
{
    Task<Result<OverviewSummaryDto>> GetOverviewAsync(
        int userId,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken = default);

    Task<Result<StatisticsDto>> GetStatisticsAsync(
        int userId,
        PeriodType period,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken = default);
}