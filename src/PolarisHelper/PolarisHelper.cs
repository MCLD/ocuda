using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clc.Polaris.Api;
using Clc.Polaris.Api.Configuration;
using Clc.Polaris.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocuda.PolarisHelper.Models;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.PolarisHelper
{
    public class PolarisHelper : IPolarisHelper
    {
        private const int CacheCodesHours = 1;
        private const int PAPIInvalidEmailErrorCode = -3518;

        private readonly IOcudaCache _cache;
        private readonly ILogger<PolarisHelper> _logger;
        private readonly IPapiClient _papiClient;

        public PolarisHelper(IOcudaCache cache,
            ILogger<PolarisHelper> logger,
            IOptions<PapiSettings> options)
        {

            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(options);

            _cache = cache;
            _logger = logger;
            _papiClient = new PapiClient(options.Value);
        }

        public PatronValidateResult AuthenticatePatron(string barcode, string password)
        {
            return _papiClient.PatronValidate(barcode, password)?.Data;
        }

        public async Task<string> GetPatronCodeNameAsync(int patronCodeId)
        {
            var patronCodes = await _cache
                .GetObjectFromCacheAsync<List<PatronCodeRow>>(Cache.PolarisPatronCodes);

            if (patronCodes == null)
            {
                patronCodes = _papiClient.PatronCodesGet().Data.PatronCodesRows;
                await _cache.SaveToCacheAsync(Cache.PolarisPatronCodes,
                    patronCodes,
                    CacheCodesHours);
            }

            return patronCodes
                .Where(_ => _.PatronCodeID == patronCodeId)
                .Select(_ => _.Description)
                .SingleOrDefault();
        }

        //public async Task<>-

        public PatronData GetPatronData(string barcode, string password)
        {
            return _papiClient.PatronBasicDataGet(barcode, password, true).Data.PatronBasicData;
        }

        public PatronData GetPatronDataOverride(string barcode)
        {
            return _papiClient.PatronBasicDataGet(barcode, addresses: true, notes: true)
                .Data
                .PatronBasicData;
        }

        public RenewRegistrationResult RenewPatronRegistration(string barcode, string email)
        {
            var date = DateTime.Now.AddYears(1);
            var updateParams = new PatronUpdateParams
            {
                BranchId = _papiClient.OrganizationId,
                UserId = _papiClient.UserId,
                LogonWorkstationId = _papiClient.WorkstationId,
                ExpirationDate = date,
                AddrCheckDate = date,
                EmailAddress = email
            };

            var renewResult = new RenewRegistrationResult();

            var updateResults = _papiClient.PatronUpdate(barcode, updateParams);

            if (updateResults.Exception != null)
            {
                _logger.LogCritical("PAPI call was not successful: {ErrorMessage}",
                    updateResults.Exception.Message);
            }
            else if (!updateResults.Response.IsSuccessStatusCode)
            {
                _logger.LogCritical("PAPI call was not successful after {Elapsed} ms: {StatusCode}",
                    updateResults.ResponseTime,
                    updateResults.Response.StatusCode);
            }
            else
            {
                if (updateResults.Data.PAPIErrorCode == PAPIInvalidEmailErrorCode)
                {
                    renewResult.EmailNotUpdated = true;
                    updateParams.EmailAddress = null;

                    updateResults = _papiClient.PatronUpdate(barcode, updateParams);
                }

                if (updateResults.Data.PAPIErrorCode != 0)
                {
                    _logger.LogCritical("PAPI error after {Elapsed} ms: {PAPIErrorCode}",
                        updateResults.ResponseTime,
                        updateResults.Data.PAPIErrorCode);
                }
                else if (updateResults.Data.PAPIErrorCode == 0)
                {
                    renewResult.Success = true;
                    if (renewResult.EmailNotUpdated)
                    {
                        _logger.LogWarning("Unable to update email to {EmailAddress} for barcode {Barcode}",
                            email,
                            barcode);
                    }
                }
            }

            return renewResult;
        }
    }
}
