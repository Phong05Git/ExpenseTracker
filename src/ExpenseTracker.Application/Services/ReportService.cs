using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Reports;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class ReportService(
    IStatisticsRepository statisticsRepository) : IReportService
{
    public async Task<Result<OverviewSummaryDto>> GetOverviewAsync(
        int userId,
        DateTimeOffset referenceDate,
        CancellationToken cancellationToken = default)
    {
        var year = referenceDate.UtcDateTime.Year;
        var month = referenceDate.UtcDateTime.Month;

        var startDate = new DateTime(
            year,
            month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var endDate = startDate.AddMonths(1);

        var totals = await statisticsRepository.GetTotalsAsync(
            userId,
            startDate,
            endDate,
            cancellationToken);

        var result = new OverviewSummaryDto
        {
            TotalIncome = totals.TotalIncome,
            TotalExpense = totals.TotalExpense,
            Balance = totals.TotalIncome - totals.TotalExpense
        };

        return Result<OverviewSummaryDto>.Success(result);
    }

    public async Task<Result<StatisticsDto>> GetStatisticsAsync(
        int userId,
        PeriodType period,
        DateTimeOffset referenceDate,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        CancellationToken cancellationToken = default)
    {
        var isCustomRange =
            startDate.HasValue &&
            endDate.HasValue;

        DateTime rangeStartDate;
        DateTime rangeEndDate;
        DateTime responseEndDate;

        if (isCustomRange)
        {
            rangeStartDate = DateTime.SpecifyKind(
                startDate!.Value.UtcDateTime.Date,
                DateTimeKind.Utc);

            var customEndDate = endDate!.Value.UtcDateTime.Date;

            rangeEndDate = DateTime.SpecifyKind(
                customEndDate.AddDays(1),
                DateTimeKind.Utc);

            responseEndDate = rangeEndDate.AddDays(-1);
        }
        else
        {
            (rangeStartDate, rangeEndDate) =
                GetPeriodRange(period, referenceDate);

            responseEndDate = rangeEndDate;
        }

        var totals = await statisticsRepository.GetTotalsAsync(
            userId,
            rangeStartDate,
            rangeEndDate,
            cancellationToken);

        var expenseCategoryBreakdown =
            await statisticsRepository.GetCategoryBreakdownAsync(
                userId,
                rangeStartDate,
                rangeEndDate,
                TransactionType.Expense,
                cancellationToken);

        var incomeCategoryBreakdown =
            await statisticsRepository.GetCategoryBreakdownAsync(
                userId,
                rangeStartDate,
                rangeEndDate,
                TransactionType.Income,
                cancellationToken);

        var totalExpense = totals.TotalExpense;
        var totalIncome = totals.TotalIncome;

        var expenseCategoryResult = expenseCategoryBreakdown
            .Select(x => new CategoryBreakdownDto
            {
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                CategoryColor = x.CategoryColor,
                Amount = x.Amount,
                Percentage = totalExpense <= 0
                    ? 0
                    : Math.Round(
                        x.Amount / totalExpense * 100m,
                        2)
            })
            .ToList();

        var incomeCategoryResult = incomeCategoryBreakdown
            .Select(x => new CategoryBreakdownDto
            {
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                CategoryColor = x.CategoryColor,
                Amount = x.Amount,
                Percentage = totalIncome <= 0
                    ? 0
                    : Math.Round(
                        x.Amount / totalIncome * 100m,
                        2)
            })
            .ToList();

        var timeline = period == PeriodType.Year && !isCustomRange
            ? await statisticsRepository.GetMonthlyStatisticsAsync(
                userId,
                rangeStartDate,
                rangeEndDate,
                cancellationToken)
            : await statisticsRepository.GetDailyStatisticsAsync(
                userId,
                rangeStartDate,
                rangeEndDate,
                cancellationToken);

        var timelineResult = isCustomRange
            ? BuildCustomTimeline(
                rangeStartDate,
                rangeEndDate,
                timeline)
            : BuildTimeline(
                period,
                rangeStartDate,
                timeline);

        var result = new StatisticsDto
        {
            Period = period,
            StartDate = rangeStartDate,
            EndDate = responseEndDate,
            TotalIncome = totals.TotalIncome,
            TotalExpense = totals.TotalExpense,
            Timeline = timelineResult,
            ExpenseCategoryBreakdown = expenseCategoryResult,
            IncomeCategoryBreakdown = incomeCategoryResult
        };

        return Result<StatisticsDto>.Success(result);
    }

    private static (DateTime StartDate, DateTime EndDate) GetPeriodRange(
        PeriodType period,
        DateTimeOffset referenceDate)
    {
        var date = referenceDate.UtcDateTime.Date;

        return period switch
        {
            PeriodType.Week => GetWeekRange(date),
            PeriodType.Month => GetMonthRange(date),
            PeriodType.Year => GetYearRange(date),
            _ => throw new ArgumentOutOfRangeException(
                nameof(period),
                period,
                null)
        };
    }

    private static (DateTime StartDate, DateTime EndDate) GetWeekRange(
        DateTime date)
    {
        var daysFromMonday =
            ((int)date.DayOfWeek + 6) % 7;

        var startDate =
            date.AddDays(-daysFromMonday);

        return (
            DateTime.SpecifyKind(
                startDate,
                DateTimeKind.Utc),
            DateTime.SpecifyKind(
                startDate.AddDays(7),
                DateTimeKind.Utc));
    }

    private static (DateTime StartDate, DateTime EndDate) GetMonthRange(
        DateTime date)
    {
        var startDate = new DateTime(
            date.Year,
            date.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        return (
            startDate,
            startDate.AddMonths(1));
    }

    private static (DateTime StartDate, DateTime EndDate) GetYearRange(
        DateTime date)
    {
        var startDate = new DateTime(
            date.Year,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);

        return (
            startDate,
            startDate.AddYears(1));
    }

    private static IReadOnlyList<StatisticsPointDto> BuildTimeline(
        PeriodType period,
        DateTime startDate,
        IReadOnlyList<(DateTime Date, decimal Income, decimal Expense)> data)
    {
        var lookup = data.ToDictionary(
            x => x.Date,
            x => (x.Income, x.Expense));

        var points = new List<StatisticsPointDto>();

        if (period == PeriodType.Year)
        {
            for (var month = 0; month < 12; month++)
            {
                var date = startDate.AddMonths(month);

                lookup.TryGetValue(
                    date,
                    out var value);

                points.Add(new StatisticsPointDto
                {
                    Date = date,
                    Income = value.Income,
                    Expense = value.Expense
                });
            }

            return points;
        }

        var numberOfDays =
            period == PeriodType.Week
                ? 7
                : DateTime.DaysInMonth(
                    startDate.Year,
                    startDate.Month);

        for (var day = 0; day < numberOfDays; day++)
        {
            var date =
                startDate.AddDays(day);

            lookup.TryGetValue(
                date,
                out var value);

            points.Add(new StatisticsPointDto
            {
                Date = date,
                Income = value.Income,
                Expense = value.Expense
            });
        }

        return points;
    }

    private static IReadOnlyList<StatisticsPointDto> BuildCustomTimeline(
        DateTime startDate,
        DateTime endDate,
        IReadOnlyList<(DateTime Date, decimal Income, decimal Expense)> data)
    {
        var lookup = data.ToDictionary(
            x => x.Date,
            x => (x.Income, x.Expense));

        var points = new List<StatisticsPointDto>();

        for (var date = startDate;
             date < endDate;
             date = date.AddDays(1))
        {
            lookup.TryGetValue(
                date,
                out var value);

            points.Add(new StatisticsPointDto
            {
                Date = date,
                Income = value.Income,
                Expense = value.Expense
            });
        }

        return points;
    }
}