using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Clc.Polaris.Api;
using Clc.Polaris.Api.Configuration;
using Clc.Polaris.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.PolarisHelper.Models;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.PolarisHelper
{
    public class PolarisHelper : IPolarisHelper
    {
        public bool IsConfigured { get; }

        private const int CacheCodesHours = 1;
        private const int PAPIInvalidEmailErrorCode = -3518;

        private readonly IOcudaCache _cache;
        private readonly IConfiguration _config;
        private readonly PolarisContext _context;
        private readonly ILogger<PolarisHelper> _logger;
        private readonly IPapiClient _papiClient;

        public PolarisHelper(IOcudaCache cache,
            IConfiguration config,
            ILogger<PolarisHelper> logger)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(logger);

            _cache = cache;
            _config = config;
            _logger = logger;

            var settings = new PapiSettings();
            _config.GetSection(PapiSettings.SECTION_NAME).Bind(settings);
            _papiClient = new PapiClient(settings);
            _papiClient.AllowStaffOverrideRequests = false;

            IsConfigured = ValidateConfiguration();
        }

        public PolarisHelper(IOcudaCache cache,
            IConfiguration config,
            PolarisContext context,
            ILogger<PolarisHelper> logger)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(logger);

            _cache = cache;
            _config = config;
            _context = context;
            _logger = logger;

            var settings = new PapiSettings();
            _config.GetSection(PapiSettings.SECTION_NAME).Bind(settings);
            _papiClient = new PapiClient(settings);

            IsConfigured = ValidateConfiguration();
        }

        public bool AuthenticateCustomer(string barcode, string password)
        {
            var validateResult = _papiClient.PatronValidate(barcode, password);

            if (validateResult.Exception != null)
            {
                _logger.LogError(validateResult.Exception, 
                    "Error authenticating Polaris account through PAPI");
                throw new OcudaException("Error authenticating customer");
            }

            return validateResult?.Data != null;
        }

        public async Task<List<CustomerBlock>> GetCustomerBlocksAsync(int customerId)
        {
            try
            {
                var blocks = await _context.Database
                    .SqlQuery<CustomerBlock>(@$"SELECT PS.PatronStopID AS BlockId, PSD.Description
                    FROM PatronStops as PS
                    INNER JOIN PatronStopDescriptions as PSD
                    on PS.PatronStopId = PSD.PatronStopId
                    WHERE PS.PatronID = {customerId}")
                    .ToListAsync();

                var freeTextBlocks = await _context.Database
                    .SqlQuery<CustomerBlock>(@$"SELECT NULL AS BlockId, FreeTextBlock AS Description
                    FROM PatronFreeTextBlocks
                    WHERE PatronID = {customerId}")
                    .ToListAsync();

                blocks.AddRange(freeTextBlocks);

                return blocks;
            }
            catch (Exception ex) when (ex is DbException || ex is InvalidOperationException)
            {
                _logger.LogError(ex, "Error querying Polaris blocks for patron {PatronId}",
                    customerId);
                throw new OcudaException("Error retrieving customer blocks");
            }
        }

        public async Task<string> GetCustomerCodeNameAsync(int customerCodeId)
        {
            var patronCodes = await _cache
                .GetObjectFromCacheAsync<List<PatronCodeRow>>(Cache.PolarisPatronCodes);

            if (patronCodes == null)
            {
                var patronCodesResult = _papiClient.PatronCodesGet();
                if (patronCodesResult?.Exception != null)
                {
                    _logger.LogError(patronCodesResult.Exception, 
                        "Error getting Polaris patron codes through PAPI");
                    throw new OcudaException("Error resolving patron code name");
                }

                patronCodes = patronCodesResult.Data.PatronCodesRows;
                await _cache.SaveToCacheAsync(Cache.PolarisPatronCodes,
                    patronCodes,
                    CacheCodesHours);
            }

            return patronCodes
                .Where(_ => _.PatronCodeID == customerCodeId)
                .Select(_ => _.Description)
                .SingleOrDefault();
        }

        public Customer GetCustomerData(string barcode, string password)
        {
            var patronDataResult = _papiClient.PatronBasicDataGet(barcode, password, addresses: true);

            if (patronDataResult?.Exception != null)
            {
                _logger.LogError(patronDataResult.Exception, "Error getting Polaris account data through PAPI");
                throw new OcudaException("Error accessing Polaris records");
            }

            var patronData = patronDataResult?.Data?.PatronBasicData;

            if (patronData != null)
            {
                return GetCustomerInfo(patronData);
            }

            return null;

        }

        public Customer GetCustomerDataOverride(string barcode)
        {
            var patronDataResult = _papiClient.PatronBasicDataGet(barcode, addresses: true, notes: true);
            if (patronDataResult?.Exception != null)
            {
                _logger.LogError(patronDataResult.Exception, "Error getting Polaris account data through PAPI override");
                throw new OcudaException("Error accessing Polaris records");
            }

            var patronData = patronDataResult?.Data?.PatronBasicData;

            if (patronData != null)
            {
                return GetCustomerInfo(patronData);
            }

            return null;
        }

        public RenewRegistrationResult RenewCustomerRegistration(string barcode, string email)
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
                _logger.LogError("PAPI call was not successful: {ErrorMessage}",
                    updateResults.Exception.Message);
            }
            else if (!updateResults.Response.IsSuccessStatusCode)
            {
                _logger.LogError("PAPI call was not successful after {Elapsed} ms: {StatusCode}",
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
                    _logger.LogError("PAPI error after {Elapsed} ms: {PAPIErrorCode}",
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

        private static Customer GetCustomerInfo(PatronData patronData)
        {
            var customer = new Customer
            {
                AddressVerificationDate = patronData.AddrCheckDate,
                BirthDate = patronData.BirthDate,
                BlockingNotes = patronData.PatronNotes?.BlockingStatusNotes,
                ChargeBalance = patronData.ChargeBalance,
                CustomerCodeId = patronData.PatronCodeID,
                CustomerIdNumber = patronData.Barcode,
                EmailAddress = patronData.EmailAddress,
                ExpirationDate = patronData.ExpirationDate,
                Id = patronData.PatronID,
                IsBlocked = patronData.PatronSystemBlocks.Any(),
                NameFirst = patronData.NameFirst,
                NameLast = patronData.NameLast,
                Notes = patronData.PatronNotes?.NonBlockingStatusNotes,
                UserDefinedField1 = patronData.User1,
                UserDefinedField2 = patronData.User2,
                UserDefinedField3 = patronData.User3,
                UserDefinedField4 = patronData.User4,
                UserDefinedField5 = patronData.User5
            };

            var addressList = new List<CustomerAddress>();
            foreach (var address in patronData.PatronAddresses)
            {
                addressList.Add(new CustomerAddress
                {
                    AddressType = address.FreeTextLabel,
                    AddressTypeId = address.AddressTypeID,
                    City = address.City,
                    Country = address.Country,
                    CountryId = address.CountryID,
                    County = address.County,
                    Id = address.AddressId,
                    PostalCode = address.PostalCode,
                    State = address.State,
                    StreetAddressOne = address.StreetOne,
                    StreetAddressTwo = address.StreetTwo,
                    ZipPlusFour = address.ZipPlusFour,
                });
            }

            customer.Addresses = addressList;

            return customer;
        }

        private bool ValidateConfiguration()
        {
            var validConfiguration = true;

            if (string.IsNullOrEmpty(_papiClient.AccessID))
            {
                _logger.LogError("Polaris Helper is not configured: PapiSetting 'AccessID' is missing");
                validConfiguration = false;
            }
            if (string.IsNullOrWhiteSpace(_papiClient.AccessKey))
            {
                _logger.LogError("Polaris Helper is not configured: PapiSetting 'AccessKey' is missing");
                validConfiguration = false;
            }
            if (string.IsNullOrWhiteSpace(_papiClient.Hostname))
            {
                _logger.LogError("Polaris Helper is not configured: PapiSetting 'Hostname' is missing");
                validConfiguration = false;
            }

            if (_papiClient.AllowStaffOverrideRequests)
            {
                if (_papiClient.OrganizationId == 0)
                {
                    _logger.LogError("Polaris Helper is not configured: PapiSetting 'OrganizationId' is missing");
                    validConfiguration = false;
                }
                if (_papiClient.UserId == 0)
                {
                    _logger.LogError("Polaris Helper is not configured: PapiSetting 'UserId' is missing");
                    validConfiguration = false;
                }
                if (_papiClient.WorkstationId == 0)
                {
                    _logger.LogError("Polaris Helper is not configured: PapiSetting 'WorkstationId' is missing");
                    validConfiguration = false;
                }
                if (string.IsNullOrWhiteSpace(_papiClient.StaffOverrideAccount.Domain))
                {
                    _logger.LogError("Polaris Helper is not configured: PapiSetting 'StaffOverrideAccount.Domain' is missing");
                    validConfiguration = false;
                }
                if (string.IsNullOrWhiteSpace(_papiClient.StaffOverrideAccount.Password))
                {
                    _logger.LogError("Polaris Helper is not configured: PapiSetting 'StaffOverrideAccount.Password' is missing");
                    validConfiguration = false;
                }
                if (string.IsNullOrWhiteSpace(_papiClient.StaffOverrideAccount.Username))
                {
                    _logger.LogError("Polaris Helper is not configured: PapiSetting 'StaffOverrideAccount.Username' is missing");
                    validConfiguration = false;
                }
                if (string.IsNullOrWhiteSpace(_context.Database.GetConnectionString()))
                {
                    _logger.LogError("Polaris Helper is not configured: ConnectionString 'Polaris' is missing");
                    validConfiguration = false;
                }
            }

            return validConfiguration;
        }
    }
}
