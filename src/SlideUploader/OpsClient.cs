using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocuda.Ops.Models;
using Ocuda.Utility.Exceptions;

namespace Ocuda.SlideUploader
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Class is instantiated via dependency injection")]
    internal class OpsClient
    {
        private const string UploadJobPath = "/api/uploadjob";
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpsClient> _logger;
        private readonly IOptions<SlideUploaderOptions> _options;

        public OpsClient(
            IOptions<SlideUploaderOptions> options,
            HttpClient httpClient,
            ILogger<OpsClient> logger)
        {
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(options);

            _httpClient = httpClient;
            _logger = logger;
            _options = options;

            JsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private JsonSerializerOptions JsonSerializerOptions { get; }

        internal async Task<JsonResponse> UploadJobAsync(SlideUploadJob job)
        {
            var fileContent = System.IO.File.ReadAllBytes(job.Filepath);
            System.IO.Path.GetFileName(job.Filepath);

            using var apiKey = new StringContent(_options.Value.ApiKey);
            using var endDate = new StringContent(job.EndDate.ToUniversalTime().ToString("O"));
            using var fileBytes = new ByteArrayContent(fileContent, 0, fileContent.Length);
            using var set = new StringContent(job.Set);

            using var startDate = new StringContent(job.StartDate.ToUniversalTime().ToString("O"));

            using var form = new MultipartFormDataContent
            {
                { apiKey, nameof(job.ApiKey) },
                { endDate, nameof(job.EndDate) },
                {
                    fileBytes,
                    nameof(job.File),
                    System.IO.Path.GetFileName(job.Filepath)
                },
                { set, nameof(job.Set) },
                { startDate , nameof(job.StartDate) },
            };

            using var requestMessage = new HttpRequestMessage
            {
                Content = form,
                Method = HttpMethod.Post,
                RequestUri = new Uri(_httpClient.BaseAddress, UploadJobPath)
            };

            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                return new JsonResponse
                {
                    Success = false,
                    Message = $"Problem inserting image: {response.StatusCode} - {response.ReasonPhrase}"
                };
            }
            else
            {
                await using var responseStream = await response.Content.ReadAsStreamAsync();
                JsonResponse responseObject;
                try
                {
                    responseObject = await JsonSerializer
                        .DeserializeAsync<JsonResponse>(responseStream, JsonSerializerOptions);
                }
                catch (JsonException jex)
                {
                    throw new OcudaException(
                        $"Unable to decode JSON response from to {requestMessage.RequestUri}",
                        jex);
                }

                if (!string.IsNullOrEmpty(responseObject.Url))
                {
                    responseObject.Url = new Uri(_httpClient.BaseAddress, responseObject.Url)
                        .ToString();
                }
                return responseObject;
            }
        }
    }
}