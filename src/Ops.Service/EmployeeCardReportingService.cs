using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys.SiteSetting;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class EmployeeCardReportingService(
        IDateTimeProvider dateTimeProvider,
        ILogger<EmployeeCardReportingService> logger,
        IEmployeeCardResultRepository employeeCardResultRepository,
        IEmployeeCardStatsRepository employeeCardStatsRepository,
        ISiteSettingService siteSettingService)
        : IEmployeeCardReportingService
    {
        public async Task<bool> AnyAsync()
        {
            return await employeeCardStatsRepository.AnyAsync();
        }

        public async Task<IEnumerable<DisplayReport>> GetReportAsync(ReportCriteria criteria)
        {
            ArgumentNullException.ThrowIfNull(criteria);

            var data = await employeeCardStatsRepository.GetReportAsync(criteria.StartDate);

            return [

                new DisplayReport(
                $"{criteria.Report.Name} {data.First().Year}",
                dateTimeProvider.Now)
                {
                    HeaderRow = ["Month",
                        "Accepted",
                        "Processed",
                        "Processed (no email)",
                        "Total Requests",
                        "Renewals",
                        "Stats collected",
                    ],
                    Data = data.Select(_ => new List<object>
                    {
                        $"{_.Month}/{_.Year}",
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? _.Accepted
                            : _.Accepted
                                .ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? _.Processed
                            : _.Processed
                                .ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? _.ProcessedNoEmail
                            : _.ProcessedNoEmail
                                .ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? _.Total
                            : _.Total.ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? _.Renewal
                            : _.Renewal.ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        _.CreatedAt.ToString(CultureInfo.CurrentCulture),
                    }),
                    FooterRow = [
                        "Total",
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? data.Sum(_ => _.Accepted)
                            : data.Sum(_ => _.Accepted)
                                .ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? data.Sum(_ => _.Processed)
                            : data.Sum(_ => _.Processed)
                                .ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? data.Sum(_ => _.ProcessedNoEmail)
                            : data.Sum(_ => _.ProcessedNoEmail)
                                .ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? data.Sum(_ => _.Total)
                            : data.Sum(_ => _.Total)
                                .ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        string.IsNullOrEmpty(criteria.NumberFormat)
                            ? data.Sum(_ => _.Renewal)
                            : data.Sum(_ => _.Renewal)
                                .ToString(criteria.NumberFormat, CultureInfo.CurrentCulture),
                        ],
                }

            ];
        }

        public async Task<DataWithCount<IDictionary<DateTime, int?>>> GetStatsAsync(
            BaseFilter<string> filter)
        {
            return new DataWithCount<IDictionary<DateTime, int?>>
            {
                Count = await employeeCardStatsRepository.GetDateCountAsync(filter),
                Data = await employeeCardStatsRepository.GetDatesAsync(filter),
            };
        }

        public async Task RunPendingReportsAsync()
        {
            var timer = Stopwatch.StartNew();
            var determineReports = await employeeCardResultRepository.GetDatesAfterAsync(
                await employeeCardStatsRepository.GetLatestDateAsync() ?? DateTime.MinValue);

            var now = dateTimeProvider.Now;

            var lookbackDays = await siteSettingService.GetSettingIntAsync(RenewCard.StatsLookback);

            var runReportsPriorTo = now.AddDays(Math.Abs(lookbackDays) * -1);

            var reportsToRun = determineReports.Where(_ => _ <= runReportsPriorTo).Order();

            if (reportsToRun?.Count() > 0)
            {
                logger.LogInformation(
                    "Found {ReportsToRunCount} employee card stats reports prior to {Date} ({Lookback} lookback) to run",
                    reportsToRun?.Count(),
                    runReportsPriorTo.ToShortDateString(),
                    Math.Abs(lookbackDays) * -1);

                foreach (var date in reportsToRun)
                {
                    var startedAt = timer.ElapsedMilliseconds;
                    var resultStats = await employeeCardResultRepository.GetStatsAsync(date);

                    var responseStats = new EmployeeCardStats
                    {
                        Accepted = resultStats.Count(
                            _ => _.Type == EmployeeCardResult.ResultType.CardCreated),
                        CreatedAt = now,
                        Month = date.Month,
                        Processed = resultStats.Count(
                            _ => _.Type == EmployeeCardResult.ResultType.Processed),
                        ProcessedNoEmail = resultStats.Count(
                            _ => _.Type == EmployeeCardResult.ResultType.ProcessedNoEmail),
                        Renewal = resultStats.Count(_ => _.Renewal),
                        Total = resultStats.Count,
                        Year = date.Year,
                    };

                    await employeeCardStatsRepository.SaveStatsAsync(responseStats);

                    logger.LogInformation(
                        "Finished running employee card stats report for {Month}/{Year} in {ElapsedMs} ms",
                        date.Month,
                        date.Year,
                        timer.ElapsedMilliseconds - startedAt);
                }

                logger.LogInformation(
                    "Finished all {Count} employee card stats reports in {ElapsedMs} ms",
                    reportsToRun.Count(),
                    timer.ElapsedMilliseconds);
            }
        }
    }
}
