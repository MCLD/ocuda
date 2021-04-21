﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Models.Screenly.v11;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service.Clients
{
    public class ScreenlyClient : IScreenlyClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ScreenlyClient> _logger;

        public ScreenlyClient(HttpClient httpClient,
            ILogger<ScreenlyClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public Task<string> AddSlideAsync(DigitalDisplay display,
            string filePath,
            AssetModel assetModel)
        {
            if (display == null)
            {
                throw new ArgumentNullException(nameof(display));
            }
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (assetModel == null)
            {
                throw new ArgumentNullException(nameof(assetModel));
            }

            return AddSlideInternalAsync(display, filePath, assetModel);
        }

        public Task<AssetModel[]> GetCurrentSlidesAsync(DigitalDisplay display)
        {
            if (display == null)
            {
                throw new ArgumentNullException(nameof(display));
            }
            return GetCurrentSlidesInternalAsync(display);
        }

        public Task<AssetModel> GetSlideAsync(DigitalDisplay display, string assetId)
        {
            if (display == null)
            {
                throw new ArgumentNullException(nameof(display));
            }
            if (string.IsNullOrEmpty(assetId))
            {
                throw new ArgumentNullException(nameof(assetId));
            }

            return GetSlideInternalAsync(display, assetId);
        }

        public Task<string> RemoveSlideAsync(DigitalDisplay display, string assetId)
        {
            if (display == null)
            {
                throw new ArgumentNullException(nameof(display));
            }
            if (string.IsNullOrEmpty(assetId))
            {
                throw new ArgumentNullException(nameof(assetId));
            }
            return RemoveSlideInternalAsync(display, assetId);
        }

        public Task UpdateSlideAsync(DigitalDisplay display,
            string assetId,
            AssetModel assetModel)
        {
            if (display == null)
            {
                throw new ArgumentNullException(nameof(display));
            }
            if (string.IsNullOrEmpty(assetId))
            {
                throw new ArgumentNullException(nameof(assetId));
            }
            if (assetModel == null)
            {
                throw new ArgumentNullException(nameof(assetModel));
            }
            return UpdateSlideInternalAsync(display, assetId, assetModel);
        }

        private static Uri UriAssetsv11(Uri remoteAddress)
        {
            return UriAssetsv11(remoteAddress, null);
        }

        private static Uri UriAssetsv11(Uri remoteAddress, string assetId)
        {
            return new UriBuilder(remoteAddress)
            {
                Path = string.IsNullOrEmpty(assetId)
                    ? "api/v1.1/assets"
                    : $"api/v1.1/assets/{assetId}"
            }.Uri;
        }

        private static Uri UriFileAssetv1(Uri remoteAddress)
        {
            return new UriBuilder(remoteAddress)
            {
                Path = "api/v1/file_asset"
            }.Uri;
        }

        private async Task<string> AddSlideInternalAsync(DigitalDisplay display,
            string filePath,
            AssetModel assetModel)
        {
            using var form = new MultipartFormDataContent();
            using var file = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));
            file.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file_upload",
                FileName = assetModel.Name
            };
            form.Add(file);

            SetBasicAuthentication(display.BasicAuthentication);

            using var uploadFileRequest = new HttpRequestMessage
            {
                Content = form,
                Method = HttpMethod.Post,
                RequestUri = UriFileAssetv1(display.RemoteAddress)
            };

            var uploadFileResponse = await _httpClient.SendAsync(uploadFileRequest);

            if (!uploadFileResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Http upload of {FilePath} to Screenly failed: {StatusCode} - {RequestMessage}",
                    filePath,
                    uploadFileResponse.StatusCode,
                    uploadFileResponse.RequestMessage);
                throw new OcudaException($"Unable to upload file to Screenly: {uploadFileResponse.StatusCode} - {uploadFileResponse.RequestMessage}");
            }

            assetModel.Uri = await uploadFileResponse.Content.ReadAsStringAsync();
            assetModel.Uri = assetModel.Uri.Trim('"');

            using var assetModelContent
                = new StringContent(JsonSerializer.Serialize(assetModel));
            using var addAssetRequest = new HttpRequestMessage
            {
                RequestUri = UriAssetsv11(display.RemoteAddress),
                Method = HttpMethod.Post,
                Content = assetModelContent
            };

            using var addAssetResponse = await _httpClient.SendAsync(addAssetRequest,
                HttpCompletionOption.ResponseHeadersRead);

            if (!addAssetResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Http add of slide details for {FilePath} failed: {StatusCode} - {RequestMessage}",
                    filePath,
                    uploadFileResponse.StatusCode,
                    uploadFileResponse.RequestMessage);
                throw new OcudaException($"Unable to add slide details to Screenly: {addAssetResponse.StatusCode} - {addAssetResponse.RequestMessage}");
            }

            using var addAssetStream = await addAssetResponse.Content.ReadAsStreamAsync();
            var added = await JsonSerializer.DeserializeAsync<AssetModel>(addAssetStream);

            return added.AssetId;
        }

        private async Task<AssetModel[]> GetCurrentSlidesInternalAsync(DigitalDisplay display)
        {
            SetBasicAuthentication(display.BasicAuthentication);

            using var getSlidesRequest = new HttpRequestMessage
            {
                RequestUri = UriAssetsv11(display.RemoteAddress),
            };

            var getSlidesResponse = await _httpClient.SendAsync(getSlidesRequest,
                HttpCompletionOption.ResponseHeadersRead);

            if (!getSlidesResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Http fetch of slides from {DisplayName} Screenly failed: {StatusCode} - {RequestMessage}",
                    display.Name,
                    getSlidesResponse.StatusCode,
                    getSlidesResponse.RequestMessage);
                throw new OcudaException($"Unable to get slides from Screenly: {getSlidesResponse.StatusCode} - {getSlidesResponse.RequestMessage}");
            }

            using var getSlidesStream = await getSlidesResponse.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<AssetModel[]>(getSlidesStream);
        }

        private async Task<AssetModel> GetSlideInternalAsync(DigitalDisplay display,
            string assetId)
        {
            SetBasicAuthentication(display.BasicAuthentication);

            using var getSlideRequest = new HttpRequestMessage
            {
                RequestUri = UriAssetsv11(display.RemoteAddress, assetId)
            };

            var getSlideResponse = await _httpClient.SendAsync(getSlideRequest,
                HttpCompletionOption.ResponseHeadersRead);

            if (!getSlideResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Http fetch of slide {AssetId} from {DisplayName} Screenly failed: {StatusCode} - {RequestMessage}",
                    assetId,
                    display.Name,
                    getSlideResponse.StatusCode,
                    getSlideResponse.RequestMessage);
                throw new OcudaException($"Unable to find asset id {assetId} in Screenly: {getSlideResponse.StatusCode} - {getSlideResponse.RequestMessage}");
            }

            using var getSlideStream = await getSlideResponse.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<AssetModel>(getSlideStream);
        }

        private async Task<string> RemoveSlideInternalAsync(DigitalDisplay display, string assetId)
        {
            SetBasicAuthentication(display.BasicAuthentication);

            using var removeSlideRequest = new HttpRequestMessage
            {
                RequestUri = UriAssetsv11(display.RemoteAddress, assetId),
                Method = HttpMethod.Delete
            };

            var removeSlideResponse = await _httpClient.SendAsync(removeSlideRequest);

            if (!removeSlideResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Http remove of slide {AssetId} from {DisplayName} Screenly failed: {StatusCode} - {RequestMessage}",
                    assetId,
                    display.Name,
                    removeSlideResponse.StatusCode,
                    removeSlideResponse.RequestMessage);
                throw new OcudaException($"Unable to remove asset id {assetId} from Screenly: {removeSlideResponse.StatusCode} - {removeSlideResponse.RequestMessage}");
            }

            return removeSlideResponse.ReasonPhrase;
        }

        private void SetBasicAuthentication(string basicAuthentication)
        {
            if (string.IsNullOrEmpty(basicAuthentication))
            {
                return;
            }

            var encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes(basicAuthentication));

            if (_httpClient.DefaultRequestHeaders.Authorization != null
                && _httpClient.DefaultRequestHeaders.Authorization.Scheme != "Basic"
                && _httpClient.DefaultRequestHeaders.Authorization.Parameter != encoded)
            {
                _logger.LogError("Attempt to reconfigure authentication with Screenly failed - HttpClient already configured");
                throw new OcudaException("Once Screenly authentication has been configured it cannot be changed.");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Basic", encoded);
            }
        }

        private async Task UpdateSlideInternalAsync(DigitalDisplay display,
                    string assetId,
            AssetModel assetModel)
        {
            var jsonSlide = new StringContent(JsonSerializer.Serialize(assetModel));

            using var updateSlideRequest = new HttpRequestMessage
            {
                RequestUri = UriAssetsv11(display.RemoteAddress, assetId),
                Method = HttpMethod.Put,
                Content = jsonSlide
            };

            using var updateSlideResponse = await _httpClient.SendAsync(updateSlideRequest,
                HttpCompletionOption.ResponseHeadersRead);

            if (!updateSlideResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Http update of slide {AssetId} from {DisplayName} Screenly failed: {StatusCode} - {RequestMessage}",
                   assetId,
                   display.Name,
                   updateSlideResponse.StatusCode,
                   updateSlideResponse.RequestMessage);
                throw new OcudaException($"Unable to update asset id {assetId} in Screenly: {updateSlideResponse.StatusCode} - {updateSlideResponse.RequestMessage}");
            }
        }
    }
}