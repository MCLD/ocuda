using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;

namespace TrestleHelper
{
    public class TrestleClient : IAddressLookupHelper
    {
        public static readonly string Source = nameof(TrestleClient);
        private const string QueryApiKey = "api_key";
        private const string QueryPostalCode = "postal_code";
        private const string QueryStreetLine1 = "street_line_1";
        private readonly HttpClient _httpClient;
        private readonly ILogger<TrestleClient> _logger;
        private readonly TrestleSettings _settings;

        public TrestleClient(HttpClient httpClient,
            IConfiguration config,
            ILogger<TrestleClient> logger)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(logger);

            _httpClient = httpClient;
            _logger = logger;

            _settings = new TrestleSettings();
            config.GetSection(TrestleSettings.SectionName).Bind(_settings);
            IsConfigured = ValidateConfiguration();
        }

        public bool IsConfigured { get; }

        public async Task<IEnumerable<AddressAssociation>>
            GetAssociatedEntitiesAsync(string address, string zip)
        {
            if (!IsConfigured)
            {
                throw new OcudaException("Lookup client is not configured, lookup failed");
            }

            var uri = new Uri(QueryHelpers.AddQueryString(_settings.ReverseAddressEndpoint,
                new Dictionary<string, string?> {
                    { QueryApiKey, _settings.Key },
                    { QueryPostalCode, zip },
                    { QueryStreetLine1, address }
            }));

            var response = await GetAsync<TrestleResponse>(uri)
                ?? throw new OcudaException("Address not found or lookup is broken.");

            if (response?.IsValid != true)
            {
                if (response?.Warnings != null)
                {
                    throw new OcudaException($"Error on lookup: {string.Join(", ", response.Warnings)}");
                }
                throw new OcudaException("Error on lookup: address is not valid.");
            }

            var associations = new List<AddressAssociation>();

            if (response?.CurrentResidents != null && response.CurrentResidents.Length > 0)
            {
                associations.Add(new AddressAssociation
                {
                    Entities = [.. response.CurrentResidents.Select(_ => _.Name ?? "Unknown name")],
                    PostalCode = response.PostalCode ?? zip,
                    PropertyType = response.IsCommercial.HasValue
                        ? response.IsCommercial.Value ? "Commercial" : "Non-commercial"
                        : "Unknown",
                    StreetAddress1 = response.StreetLine1 ?? address
                });
            }

            return associations;
        }

        private async Task<T> GetAsync<T>(Uri uri)
        {
            string? responseText = null;
            var sanitizedUri = uri.AbsoluteUri.Replace(_settings.Key,
                "--APIKEY--",
                StringComparison.OrdinalIgnoreCase);

            try
            {
                using var response = await _httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    responseText = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }

                var responseStream = await response.Content.ReadAsStreamAsync();
                if (responseStream != null)
                {
                    var result = await JsonSerializer.DeserializeAsync<T>(responseStream);

                    return result == null
                        ? throw new OcudaException("Response was not formatted correctly.")
                        : result;
                }

                throw new OcudaException("Response stream was empty.");
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                var sanitizedResponse = responseText?.Replace(_settings.Key,
                        "--APIKEY--",
                        StringComparison.OrdinalIgnoreCase);
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    {"TrestleRequestUri", sanitizedUri},
                    {"TrestleResponseText", sanitizedResponse ?? ""}
                }))
                {
                    _logger.LogWarning(ex, "Error in Web query: {ErrorMessage}", ex.Message);
                }
                throw new OcudaException($"Error: {ex.Message}", ex);
            }
        }

        private bool ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(_settings.Key))
            {
                _logger.LogWarning("Setting {SettingName} is not configured.",
                    nameof(_settings.Key));
                return false;
            }
            else if (string.IsNullOrEmpty(_settings.ReverseAddressEndpoint))
            {
                _logger.LogWarning("Setting {SettingName} is not configured.",
                    nameof(_settings.ReverseAddressEndpoint));
                return false;
            }

            return true;
        }
    }
}