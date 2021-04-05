using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Utility.Exceptions;

namespace Ocuda.SlideUploader
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance",
        "CA1812:Avoid uninstantiated internal classes",
        Justification = "Class is instantiated via dependency injection")]
    internal class OpsClient
    {
        private const string UploadJobPath = "ContentManagement/DigitalDisplays/UploadJob";
        private const string WhoamiPath = "whoami";
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpsClient> _logger;

        public OpsClient(HttpClient httpClient, ILogger<OpsClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        internal async Task<string> IsAuthenticatedAsync()
        {
            using var requestMessage = new HttpRequestMessage
            {
                RequestUri = new Uri(_httpClient.BaseAddress, WhoamiPath)
            };
            var response = await _httpClient.SendAsync(requestMessage,
                HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogCritical("Could not authenticate to Ops: {StatusCode} - {ReasonPhrase}",
                    response.StatusCode,
                    response.ReasonPhrase);
            }

            try
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<UserInformation>(responseText,
                     new JsonSerializerOptions
                     {
                         PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                     });
                if (responseObject.Authenticated)
                {
                    return responseObject.Username;
                }
            }
            catch (JsonException)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        internal async Task<JsonResponse> UploadJobAsync(SlideUploadJob job)
        {
            var fileContent = System.IO.File.ReadAllBytes(job.Filepath);
            System.IO.Path.GetFileName(job.Filepath);

            using var startDate = new StringContent(job.StartDate.ToUniversalTime().ToString("O"));
            using var endDate = new StringContent(job.EndDate.ToUniversalTime().ToString("O"));
            using var set = new StringContent(job.Set);

            using var fileBytes = new ByteArrayContent(fileContent, 0, fileContent.Length);

            using var form = new MultipartFormDataContent
            {
                { startDate , nameof(job.StartDate) },
                { endDate, nameof(job.EndDate) },
                { set, nameof(job.Set) },
                {
                    fileBytes,
                    nameof(job.File),
                    System.IO.Path.GetFileName(job.Filepath)
                }
            };

            using var requestMessage = new HttpRequestMessage
            {
                Content = form,
                Method = HttpMethod.Post,
                RequestUri = new Uri(_httpClient.BaseAddress, UploadJobPath)
            };

            var response = await _httpClient.SendAsync(requestMessage,
                HttpCompletionOption.ResponseHeadersRead);

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
                using var responseStream = await response.Content.ReadAsStreamAsync();
                JsonResponse responseObject;
                try
                {
                    responseObject = await JsonSerializer
                        .DeserializeAsync<JsonResponse>(responseStream,
                            new JsonSerializerOptions
                            {
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                            });
                }
                catch (JsonException jex)
                {
                    throw new OcudaException($"Unable to decode JSON response from to {requestMessage.RequestUri}",
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