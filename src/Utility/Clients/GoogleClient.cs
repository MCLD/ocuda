using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;
using Ocuda.Utility.Models.Google;

namespace Ocuda.Utility.Clients
{
    public class GoogleClient : IGoogleClient
    {
        private const string GeocodeLink = "https://maps.googleapis.com/maps/api/geocode/json";
        private const string PlaceDetailsLink = "https://maps.googleapis.com/maps/api/place/details/json";
        private const string PlaceTextLink = "https://maps.googleapis.com/maps/api/place/textsearch/json";

        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleClient> _logger;

        public GoogleClient(HttpClient httpClient,
            IConfiguration config,
            ILogger<GoogleClient> logger)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient.Timeout = TimeSpan.FromSeconds(15);

            _apiKey = config[Configuration.OcudaGoogleAPI];
        }

        public Task<(double? Latitude, double? Longitude)> GeocodeAsync(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                throw new ArgumentNullException(nameof(address));
            }

            return GeocodeInternalAsync(address);
        }

        public async Task<string> GetLocationLinkAsync(string placeId)
        {
            var uri = new Uri(QueryHelpers.AddQueryString(PlaceDetailsLink,
                new Dictionary<string, string> {
                    { "placeid", HttpUtility.UrlEncode(placeId) },
                    { "key", HttpUtility.UrlEncode(_apiKey) }
                }));

            var response = await GetAsync<GoogleSingleResponse<PlaceDetails>>(uri);

            if (response.Status == "OK")
            {
                if (response.Result != null)
                {
                    return response.Result.Url;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["GoogleInfoMessages"] = response.InfoMessages,
                    ["GoogleStatus"] = response.Status
                }))
                {
                    _logger.LogWarning("Google API place details failed for: {PlaceId},", placeId);
                }
                throw new OcudaException($"Place details lookup failed with status {response.Status}");
            }
        }

        public async Task<ICollection<LocationSummary>> GetLocationSummariesAsync(string query)
        {
            var uri = new Uri(QueryHelpers.AddQueryString(PlaceTextLink,
                new Dictionary<string, string> {
                    { "query", HttpUtility.UrlEncode(query) },
                    { "key", HttpUtility.UrlEncode(_apiKey) }
                }));

            var response = await GetAsync<GoogleMultiResponse<TextSearchResult>>(uri);

            if (response.Status == "OK")
            {
                return response.Results.Select(_ => new LocationSummary
                {
                    Address = _.FormattedAddress,
                    Name = _.Name,
                    PlaceId = _.PlaceId
                }).ToList();
            }
            else
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["GoogleInfoMessages"] = response.InfoMessages,
                    ["GoogleStatus"] = response.Status
                }))
                {
                    _logger.LogWarning("Google API text search failed for: {Query},", query);
                }
                throw new OcudaException($"Location summary lookup failed with status {response.Status}");
            }
        }

        public async Task<string> GetZipCodeAsync(double latitude, double longitude)
        {
            var uri = new Uri(QueryHelpers.AddQueryString(GeocodeLink,
                new Dictionary<string, string> {
                    { "latlng", HttpUtility.UrlEncode($"{latitude},{longitude}") },
                    { "key", HttpUtility.UrlEncode(_apiKey) }
                }));

            var response = await GetAsync<GoogleMultiResponse<GeocodeResult>>(uri);

            var postalCode = Array.Find(response.Results,
                    _ => _.Types.Any(__ => __ == "postal_code"))
                .AddressComponents?
                .FirstOrDefault();

            return postalCode?.ShortName;
        }

        private async Task<(double? Latitude, double? Longitude)>
            GeocodeInternalAsync(string address)
        {
            var uri = new Uri(QueryHelpers.AddQueryString(GeocodeLink,
                new Dictionary<string, string> {
                    { "address", HttpUtility.UrlEncode(address) },
                    { "key", HttpUtility.UrlEncode(_apiKey) }
                }));

            var response = await GetAsync<GoogleMultiResponse<GeocodeResult>>(uri);

            if (response.Results?.Length > 0)
            {
                return (response.Results[0].Geometry.Location.Lat,
                    response.Results[0].Geometry.Location.Lng);
            }
            else
            {
                return (null, null);
            }
        }

        private async Task<T> GetAsync<T>(Uri uri)
        {
            string responseText = null;
            try
            {
                using var response = await _httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    responseText = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }

                var responseStream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<T>(responseStream);
            }
            catch (HttpRequestException hrex)
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["GoogleRequestUri"] = uri.AbsoluteUri.Replace(_apiKey,
                        "--APIKEY--",
                        StringComparison.OrdinalIgnoreCase),
                    ["GoogleResponseText"] = responseText?.Replace(_apiKey,
                        "--APIKEY--",
                        StringComparison.OrdinalIgnoreCase)
                }))
                {
                    _logger.LogWarning(hrex,
                        "Error Google API query: {ErrorMessage}",
                        hrex.Message);
                }
                throw new OcudaException($"Error: {hrex.Message}", hrex);
            }
            catch (JsonException jex)
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["GoogleRequestUri"] = uri
                }))
                {
                    _logger.LogWarning(jex, "Error decoding JSON: {ErrorMessage}", jex.Message);
                }
                throw new OcudaException($"Unable to decode JSON: {jex.Message}", jex);
            }
        }
    }
}