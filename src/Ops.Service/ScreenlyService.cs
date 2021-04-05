using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models.Screenly.v11;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class ScreenlyService : BaseService<ScreenlyService>, IScreenlyService
    {
        private readonly IScreenlyClient _screenlyClient;

        public ScreenlyService(ILogger<ScreenlyService> logger,
            IHttpContextAccessor httpContextAccessor,
            IScreenlyClient screenlyClient) : base(logger, httpContextAccessor)
        {
            _screenlyClient = screenlyClient
                ?? throw new ArgumentNullException(nameof(screenlyClient));
        }

        public Task<string> AddSlideAsync(DigitalDisplay display,
            string filePath,
            DigitalDisplayAssetSet assetSet)
        {
            if (display == null)
            {
                throw new ArgumentNullException(nameof(display));
            }
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (assetSet == null)
            {
                throw new ArgumentNullException(nameof(assetSet));
            }
            return AddSlideInternalAsync(display, filePath, assetSet);
        }

        public async Task<ICollection<AssetModel>> GetCurrentSlidesAsync(DigitalDisplay display)
        {
            return await _screenlyClient.GetCurrentSlidesAsync(display);
        }

        public async Task<string> RemoveSlideAsync(DigitalDisplay display, string assetId)
        {
            return await _screenlyClient.RemoveSlideAsync(display, assetId);
        }

        public Task UpdateSlideAsync(DigitalDisplay display,
            string assetId,
            DigitalDisplayAssetSet assetSet)
        {
            if (display == null)
            {
                throw new ArgumentNullException(nameof(display));
            }
            if (assetSet == null)
            {
                throw new ArgumentNullException(nameof(assetSet));
            }

            return UpdateSlideInternalAsync(display, assetId, assetSet);
        }

        private async Task<string> AddSlideInternalAsync(DigitalDisplay display,
                                    string filePath,
            DigitalDisplayAssetSet assetSet)
        {
            return await _screenlyClient.AddSlideAsync(display,
                filePath,
                new AssetModel
                {
                    Duration = "10",
                    EndDate = assetSet.EndDate.ToUniversalTime(),
                    IsActive = assetSet.IsEnabled ? 1 : 0,
                    IsEnabled = 1,
                    MimeType = "image",
                    Name = assetSet.DigitalDisplayAsset.Name,
                    StartDate = assetSet.StartDate.ToUniversalTime()
                });
        }

        private async Task UpdateSlideInternalAsync(DigitalDisplay display,
            string assetId,
            DigitalDisplayAssetSet assetSet)
        {
            var currentSlide = await _screenlyClient.GetSlideAsync(display, assetId);

            if (currentSlide == null)
            {
                throw new OcudaException($"Unable to find asset id {assetId} on display {display.Name}");
            }

            currentSlide.StartDate = assetSet.StartDate.ToUniversalTime();
            currentSlide.EndDate = assetSet.EndDate.ToUniversalTime();
            currentSlide.IsActive = assetSet.IsEnabled ? 1 : 0;

            await _screenlyClient.UpdateSlideAsync(display, assetId, currentSlide);
        }
    }
}