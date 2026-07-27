using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Abstract;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class ReportingService(ILogger<ReportingService> logger,
        IDateTimeProvider dateTimeProvider,
        IEmediaReportingService emediaReportingService,
        IEmployeeCardReportingService employeeCardReportingService,
        IHttpContextAccessor httpContextAccessor,
        IPolarisHelper polarisHelper,
        IRenewCardReportingService renewCardReportingService,
        IReportingImportDatumRepository reportingImportDatumRepository,
        IReportingImportDetailsRepository reportingImportDetailsRepository,
        IReportingImportHeaderRepository reportingImportHeaderRepository,
        IReportingLocationRepository reportingLocationRepository,
        IReportingLocationSetRepository reportingLocationSetRepository)
        : BaseService<ReportingService>(logger, httpContextAccessor), IReportingService
    {
        private static DisplayReport AdaptImportHeader(ReportCriteria criteria,
            ReportingImportHeader reportingImportHeader)
        {
            var displayReport = new DisplayReport(
                $"{criteria.Report.Name} {reportingImportHeader.Month}/{reportingImportHeader.Year}",
                reportingImportHeader.CreatedAt)
            {
                HeaderRow = ["Location", "Circulations"],
            };

            var locations = reportingImportHeader
                .ReportingLocationSet
                .ReportingLocations
                .OrderBy(_ => _.Name);

            var dataSet = new List<IEnumerable<object>>();

            foreach (var location in locations)
            {
                var dataItem = reportingImportHeader
                    .ReportingImportData
                    .SingleOrDefault(_ => _.LocationId == location.LocationId)
                    ?.ReportValue ?? 0;

                dataSet.Add([location.Name,
                    string.IsNullOrEmpty(criteria.NumberFormat)
                        ? dataItem
                        : dataItem.ToString(criteria.NumberFormat, CultureInfo.CurrentCulture)]);
            }

            displayReport.Data = dataSet;

            var total = reportingImportHeader.ReportingImportData.Sum(_ => _.ReportValue);

            displayReport.FooterRow = ["Total",
                string.IsNullOrEmpty(criteria.NumberFormat)
                ? total
                : total.ToString(criteria.NumberFormat, CultureInfo.CurrentCulture)];

            return displayReport;
        }

        /// <summary>
        /// Given a set of locations, generate a hash using the location id, name, and abbreviation.
        /// </summary>
        /// <param name="locations">The enumerable of ReportingLocation objects.</param>
        /// <returns>checksum of the specified fields of locations combined.</returns>
        private static byte[] GetLocationHash(IEnumerable<ReportingLocation> locations)
        {
            var items = new StringBuilder();
            foreach (var location in locations.OrderBy(_ => _.LocationId)
                .ThenBy(_ => _.Name)
                .ThenBy(_ => _.Abbreviation))
            {
                items.Append(location.LocationId)
                    .Append(location.Name)
                    .Append(location.Abbreviation);
            }

            return SHA256.HashData(Encoding.UTF8.GetBytes(items.ToString()));
        }

        /// <summary>
        /// Use the <see cref="ReportDefinition"/> combined with the CSV data from the stream to
        /// read in reporting data.
        /// </summary>
        /// <typeparam name="T">The DTO type to place data in, should match the columns in the CSV
        /// file or have appropriate attributes.</typeparam>
        /// <param name="report">The report definition including CSV parse details.</param>
        /// <param name="import">The CSV file as a stream.</param>
        /// <returns>An <see cref="IEnumerable"/> of the generic type for this method call.</returns>
        private static IEnumerable<T> ParseCsv<T>(ReportDefinition report, Stream import)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = report.Delimiter,
                HasHeaderRecord = true,
                ShouldSkipRecord = (shouldSkip) => report
                    .SkipFirstColumn
                    .Contains(shouldSkip.Row[0].Trim()),
            };

            using var reader = new StreamReader(import);
            using var csv = new CsvReader(reader, csvConfig);

            try
            {
                return [.. csv.GetRecords<T>()];
            }
            catch (HeaderValidationException hvex)
            {
                throw new OcudaException($"Error in supplied document headers: {hvex.Message}",
                    hvex);
            }
        }

        public async Task<DataWithCount<IDictionary<DateTime, int?>>>
            GetDetailsAsync(BaseFilter<string> filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            return filter.Data switch
            {
                ReportDefinitionId.DigitalLibraryAccesses => await emediaReportingService
                    .GetStatsAsync(filter),
                ReportDefinitionId.EmployeeCardRequests => await employeeCardReportingService
                    .GetStatsAsync(filter),
                ReportDefinitionId.OnlineCardRenewalStats => await renewCardReportingService
                    .GetStatsAsync(filter),
                ReportDefinitionId.HooplaCheckouts => new DataWithCount<IDictionary<DateTime, int?>>
                {
                    Count = await reportingImportHeaderRepository.GetDateCountAsync(filter),
                    Data = await reportingImportHeaderRepository.GetDatesAsync(filter),
                },
                _ => throw new OcudaException("Unable to find report type: {reportType}"),
            };
        }

        public CollectionWithCount<ReportDefinition> GetList()
        {
            return GetList(null);
        }

        public CollectionWithCount<ReportDefinition> GetList(BaseFilter<string> filter)
        {
            var baseQuery = ReportDefinitions.Definitions.OrderBy(_ => _.Name).AsEnumerable();

            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.Data))
                {
                    baseQuery = baseQuery.Where(_ => _.ReportType == filter.Data);
                }

                if (filter.Skip.HasValue && filter.Take.HasValue)
                {
                    baseQuery = baseQuery.Skip(filter.Skip.Value).Take(filter.Take.Value);
                }
            }

            return new CollectionWithCount<ReportDefinition>
            {
                Count = baseQuery.Count(),
                Data = [.. baseQuery],
            };
        }

        public async Task<IEnumerable<ReportingImportDetails>> GetNotesAsync(ReportCriteria criteria)
        {
            ArgumentNullException.ThrowIfNull(criteria);

            var header = await reportingImportHeaderRepository.GetReportAsync(criteria)
                ?? throw new OcudaException($"Report header for {criteria.Report.Id} {criteria.StartDate.Year}/{criteria.StartDate.Month} not found.");

            return await reportingImportDetailsRepository.GetNotesAsync(header.Id);
        }

        public async Task<IEnumerable<DisplayReport>> GetResultsAsync(ReportCriteria criteria)
        {
            ArgumentNullException.ThrowIfNull(criteria);

            switch (criteria.Report.Id)
            {
                case ReportDefinitionId.DigitalLibraryAccesses:
                    return await emediaReportingService.GetReportAsync(criteria);
                case ReportDefinitionId.EmployeeCardRequests:
                    return await employeeCardReportingService.GetReportAsync(criteria);
                case ReportDefinitionId.OnlineCardRenewalStats:
                    return await renewCardReportingService.GetReportAsync(criteria);
                case ReportDefinitionId.HooplaCheckouts:
                    var data = await reportingImportHeaderRepository.GetReportAsync(criteria)
                        ?? throw new OcudaException($"No report found for type {criteria.Report.Id} for {criteria.StartDate.Month}/{criteria.StartDate.Year}");
                    return [AdaptImportHeader(criteria, data)];
                default:
                    throw new OcudaException($"Unknown report type: {criteria.Report.Id}");
            }
        }

        public async Task<bool> HasResultsAsync(ReportCriteria criteria)
        {
            return await reportingImportHeaderRepository.HasReportAsync(criteria);
        }

        public async Task<bool> HasResultsAsync(string reportType)
        {
            return reportType switch
            {
                ReportDefinitionId.DigitalLibraryAccesses
                    => await emediaReportingService.AnyAsync(),
                ReportDefinitionId.EmployeeCardRequests
                    => await employeeCardReportingService.AnyAsync(),
                ReportDefinitionId.OnlineCardRenewalStats
                    => await renewCardReportingService.AnyAsync(),
                ReportDefinitionId.HooplaCheckouts
                    => await reportingImportHeaderRepository.AnyAsync(reportType),
                _ => throw new OcudaException("Unknown report type: {reportType}"),
            };
        }

        public async Task<int> ProcessImportAsync(string reportId,
            DateTime dataDate,
            string filename,
            Stream import)
        {
            DataWithNotes<int> saveResult;
            var timer = Stopwatch.StartNew();
            var reports = ReportDefinitions.Definitions;
            var selectedReport = reports.SingleOrDefault(_ => _.Id == reportId)
                ?? throw new OcudaException($"Unable to import data for report id: {reportId}");

            if (await HasResultsAsync(new ReportCriteria
            {
                Report = selectedReport,
                StartDate = new DateTime(dataDate.Year, dataDate.Month, 1),
            }))
            {
                throw new OcudaException($"Report {selectedReport.Name} already has data for {dataDate.Month}/{dataDate.Year}");
            }

            var notes = new List<string>();

            if (selectedReport.Id == ReportDefinitionId.HooplaCheckouts)
            {
                var result = await MapDataToLocations<HooplaCardDetail>(selectedReport,
                    import);
                notes.AddRange(result.Notes);

                result.Data.Filename = filename;
                result.Data.ReportType = selectedReport.Id;
                result.Data.Timestamp = dataDate;

                saveResult = await SaveDataAsync(result.Data);

                notes.AddRange(saveResult.Notes);
                notes.Add($"Import complete in {timer.Elapsed}");

                await reportingImportDetailsRepository.AddRangeAsync(notes
                    .ConvertAll(_ => new ReportingImportDetails
                    {
                        CreatedAt = dateTimeProvider.Now,
                        CreatedBy = GetCurrentUserId(),
                        Note = _.Length > 255 ? _[..255] : _,
                        ReportingImportHeaderId = saveResult.Data,
                    }));
                await reportingImportDetailsRepository.SaveAsync();
            }
            else
            {
                throw new OcudaException($"Import process undefined for report id: {reportId}");
            }

            return saveResult.Data;
        }

        /// <summary>
        /// Map barcode circulations provided as a list of <see cref="BarcodeCirculationBase"/>
        /// to aggregated totals by location.
        /// </summary>
        /// <param name="data">An <see cref="IEnumerable"/> of <see cref="BarcodeCirculationBase"/>
        /// data to map.</param>
        /// <param name="organizationIds">The list of organization ids to map circulations into.
        /// </param>
        /// <param name="fallbackOrganizationId">Which organization id to map circulations to
        /// if the barcode cannot be mapped to a location.</param>
        /// <returns>A <see cref="LocationCirculationResult"/> with a dictionary containing
        /// location ids mapped to circulation counts and a list of barcodes which could not be
        /// mapped.</returns>
        private async Task<LocationCirculationResult> BatchToLocationsAsync(
            IEnumerable<BarcodeCirculationBase> data,
            IEnumerable<int> organizationIds,
            int fallbackOrganizationId)
        {
            const int take = 100;

            var result = new LocationCirculationResult();

            int processedItems = 0;
            int totalItems = data.Count();
            var skip = 0;

            foreach (var organizationId in organizationIds)
            {
                result.LocationIdCirculationCount.Add(organizationId, 0);
            }

            while (processedItems < totalItems)
            {
                var barcodeBatch = data.Skip(skip).Take(take).Select(_ => _.Barcode);
                skip += take;

                if (!barcodeBatch.Any())
                {
                    break;
                }

                var barcodeOrgMap = await polarisHelper.GetOrganizationIdsBatchDirect(barcodeBatch);

                foreach (var barcode in barcodeBatch)
                {
                    var totalString = data.Single(_ => _.Barcode == barcode).Circulations;
                    if (!int.TryParse(totalString,
                        NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture,
                        out int total))
                    {
                        _logger.LogError("Unable to parse total value {Total} into a number",
                            totalString);
                    }
                    else
                    {
                        if (barcodeOrgMap.TryGetValue(barcode, out int organizationId))
                        {
                            // we found a barcode -> organization id mapping
                            result.LocationIdCirculationCount[organizationId] += total;
                        }
                        else
                        {
                            // look up barcode as former id
                            var orgIdFormer = await polarisHelper
                                .GetOrganizationIdFormerDirect(barcode);

                            if (orgIdFormer.HasValue)
                            {
                                result.LocationIdCirculationCount[orgIdFormer.Value] += total;
                            }
                            else
                            {
                                // this means that the barcode has been reassigned
                                _logger.LogWarning(
                                    "Could not find {Barcode} as current or former barcode",
                                    barcode);
                                result.NotFoundBarcodes.Add(barcode);
                                result.LocationIdCirculationCount[fallbackOrganizationId] += total;
                            }
                        }
                    }
                }

                processedItems += barcodeBatch.Count();
            }

            _logger.LogInformation(
                "Processed {Processed}/{Total} barcodes totalling {Count} circulations.",
                processedItems,
                totalItems,
                result.LocationIdCirculationCount.Sum(_ => _.Value));

            return result;
        }

        /// <summary>
        /// Fetch the latest locations from Polaris and compare them to the current LocationSet in
        /// the database. If they are different then insert an updated LocationSet and return it,
        /// otherwise return the current set.
        /// </summary>
        /// <returns>The current set of locations.</returns>
        private async Task<ReportingLocationSet> GetCurrentLocationSetAsync()
        {
            var locations = polarisHelper.GetOrganizations().Select(_ => new ReportingLocation
            {
                Abbreviation = _.Abbreviation?.Trim(),
                FallbackLocation = _.ParentOrganizationID == null,
                LocationId = _.OrganizationID,
                Name = _.Name?.Trim(),
            }).ToList();

            var hash = GetLocationHash(locations);

            var currentSet = await reportingLocationSetRepository.GetCurrentAsync();

            if (currentSet?.Sha256Checksum?.SequenceEqual(hash) == true)
            {
                currentSet.ReportingLocations = await reportingLocationRepository
                     .GetBySetIdAsync(currentSet.Id);
                return currentSet;
            }
            else
            {
                var newSet = new ReportingLocationSet
                {
                    CreatedBy = GetCurrentUserId(),
                    CreatedAt = dateTimeProvider.Now,
                    IsCurrent = true,
                    Sha256Checksum = hash,
                };

                if (currentSet != null)
                {
                    _logger.LogInformation("Mismatch of reporting locations, creating new set");
                    currentSet.IsCurrent = false;
                    reportingLocationSetRepository.Update(currentSet);
                }
                else
                {
                    _logger.LogInformation("No prior set of reporting locations, creating new set");
                }

                await reportingLocationSetRepository.AddAsync(newSet);
                await reportingLocationSetRepository.SaveAsync();

                foreach (var location in locations)
                {
                    location.ReportingLocationSetId = newSet.Id;
                }

                await reportingLocationRepository.AddRangeAsync(locations);
                await reportingLocationRepository.SaveAsync();

                newSet.ReportingLocations = locations;

                return newSet;
            }
        }

        /// <summary>
        /// Use the <see cref="ReportDefinition"/> combined with the CSV data from the stream to
        /// transform a list of barcodes and circulations to an aggregated list of locations and
        /// circulations.
        /// </summary>
        /// <typeparam name="T">A class that extends <see cref="BarcodeCirculationBase"/> with
        /// attributes that align with the CSV data in the provided stream.</typeparam>
        /// <param name="reportDefinition">Information about the report to facilitate reading the
        /// CSV data.</param>
        /// <param name="import">CSV data provided as a <see cref="Stream"/>.</param>
        /// <returns>A populated <see cref="DataWithNotes"/> containing a
        /// <see cref="LocationCirculationResult"/> and notes about the import process.</returns>
        private async Task<DataWithNotes<LocationCirculationResult>> MapDataToLocations<T>(
            ReportDefinition reportDefinition,
            Stream import)
            where T : BarcodeCirculationBase
        {
            var result = new DataWithNotes<LocationCirculationResult>();

            var locationSet = await GetCurrentLocationSetAsync();
            var fallback = locationSet.ReportingLocations.First(_ => _.FallbackLocation);
            var timer = Stopwatch.StartNew();

            // extract data from CSV file
            var data = ParseCsv<T>(reportDefinition, import);
            var totalParseCsv = data.Select(_ => int.Parse(_.Circulations,
                    NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture)).Sum(_ => _);
            result.Notes.Add($"Read {data.Count()} CSV records for {totalParseCsv} circulations");
            _logger.LogInformation(
                "Read {RecordCount} {Type} for {Total} circulations in {ElapsedMs} ms",
                data.Count(),
                "CSV records",
                totalParseCsv,
                timer.ElapsedMilliseconds);

            timer.Reset();

            // map barcodes in data to locations and aggregate
            result.Data = await BatchToLocationsAsync(data,
                locationSet.ReportingLocations.Select(_ => _.LocationId),
                fallback.LocationId);

            // report on barcodes which were not found
            if (result.Data.NotFoundBarcodes.Count > 0)
            {
                result.Notes.AddRange([.. result.Data
                    .NotFoundBarcodes
                    .Select(_ => $"Barcode {_} not found, circulations attributed to {fallback.Name}")]);
            }

            var circulations = result.Data.LocationIdCirculationCount.Sum(_ => _.Value);
            result.Notes.Add($"Read {result.Data.LocationIdCirculationCount.Count} organizations with data for {circulations} circulations");
            _logger.LogInformation(
                "Read {RecordCount} {Type} for {Total} circulations in {ElapsedMs} ms",
                result.Data.LocationIdCirculationCount.Count,
                "organizations with data",
                circulations,
                timer.ElapsedMilliseconds);

            result.Data.ReportingLocationSetId = locationSet.Id;

            return result;
        }

        /// <summary>
        /// Save the result of parsing data out into the database so it can be used to generate
        /// reports.
        /// </summary>
        /// <param name="result">The result of reading, parsing, and aggregating data.</param>
        /// <returns>The integer id of the <see cref="ReportingImportHeader"/>.</returns>
        private async Task<DataWithNotes<int>> SaveDataAsync(LocationCirculationResult result)
        {
            var saveResult = new DataWithNotes<int>();

            var header = new ReportingImportHeader
            {
                CreatedAt = dateTimeProvider.Now,
                CreatedBy = GetCurrentUserId(),
                Filename = result.Filename,
                Month = result.Timestamp.Month,
                ReportingLocationSetId = result.ReportingLocationSetId,
                ReportType = result.ReportType,
                Year = result.Timestamp.Year,
            };

            await reportingImportHeaderRepository.AddAsync(header);
            await reportingImportHeaderRepository.SaveAsync();

            var data = result.LocationIdCirculationCount.Select(_ => new ReportingImportDatum
            {
                LocationId = _.Key,
                ReportingImportHeaderId = header.Id,
                ReportValue = _.Value,
            }).ToList();

            await reportingImportDatumRepository.AddRangeAsync(data);
            await reportingImportDatumRepository.SaveAsync();

            try
            {
                var total = data?.Sum(_ => _.ReportValue);
                if (total.HasValue)
                {
                    await reportingImportHeaderRepository.UpdateTotalAsync(header.Id, total.Value);
                }
            }
            catch (OcudaException oex)
            {
                _logger.LogWarning(oex,
                    "Unable to update total in header record: {ErrorMessage}",
                    oex.Message);
                saveResult.Notes.Add($"Unable to update total in report header record: {oex.Message}");
            }

            saveResult.Data = header.Id;
            saveResult.Notes.Add($"Imported {data.Count} records from file {result.Filename} for {result.Timestamp.Month}/{result.Timestamp.Year} to database");

            return saveResult;
        }

        /// <summary>
        /// This class represents the fields out of the CSV file we wish to import.
        /// </summary>
        private class HooplaCardDetail : BarcodeCirculationBase
        {
            [Name("Library Card")]
            public override string Barcode { get; set; }

            [Name("Total")]
            public override string Circulations { get; set; }
        }
    }
}