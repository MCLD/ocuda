using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models.Screenly.v11;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class DigitalDisplaySyncService
        : BaseService<DigitalDisplaySyncService>, IDigitalDisplaySyncService

    {
        private readonly IDigitalDisplayAssetSetRepository _digitalDisplayAssetSetRepository;
        private readonly IDigitalDisplayItemRepository _digitalDisplayItemRepository;
        private readonly IDigitalDisplayRepository _digitalDisplayRepository;
        private readonly IDigitalDisplayService _digitalDisplayService;
        private readonly IScreenlyService _screenlyService;

        public DigitalDisplaySyncService(ILogger<DigitalDisplaySyncService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDigitalDisplayItemRepository digitalDisplayItemRepository,
            IDigitalDisplayRepository digitalDisplayRepository,
            IDigitalDisplayService digitalDisplayService,
            IDigitalDisplayAssetSetRepository digitalDisplayAssetSetRepository,
            IScreenlyService screenlyService)
            : base(logger, httpContextAccessor)
        {
            _digitalDisplayItemRepository = digitalDisplayItemRepository
                ?? throw new ArgumentNullException(nameof(digitalDisplayItemRepository));
            _digitalDisplayRepository = digitalDisplayRepository
                ?? throw new ArgumentNullException(nameof(digitalDisplayRepository));
            _digitalDisplayService = digitalDisplayService
                ?? throw new ArgumentNullException(nameof(digitalDisplayService));
            _digitalDisplayAssetSetRepository = digitalDisplayAssetSetRepository
                ?? throw new ArgumentNullException(nameof(digitalDisplayAssetSetRepository));
            _screenlyService = screenlyService
                ?? throw new ArgumentNullException(nameof(screenlyService));
        }

        public async Task UpdateDigitalDisplaysAsync()
        {
            var display = await GetPendingDisplayAsync();

            int startingSlides = 0;
            int deleted = 0;
            int uploaded = 0;
            int updated = 0;

            try
            {
                if (display != null)
                {
                    await _digitalDisplayService.SetDisplayStatusAsync(display.Id,
                        "Connecting to update slides...");

                    await _digitalDisplayRepository
                        .UpdateLastAttemptAsync(display.Id, DateTime.Now);

                    ICollection<AssetModel> currentSlides;
                    try
                    {
                        // slides in the display based on an api request
                        currentSlides = await _screenlyService.GetCurrentSlidesAsync(display);

                        startingSlides = currentSlides.Count;

                        await _digitalDisplayRepository
                            .UpdateLastCommunicationAsync(display.Id, DateTime.Now);

                        await _digitalDisplayService.SetDisplayStatusAsync(display.Id,
                            $"Connected, found {currentSlides.Count} slides");
                    }
                    catch (OcudaException oex)
                    {
                        await _digitalDisplayRepository
                            .UpdateLastCommunicationAsync(display.Id, DateTime.Now);
                        await _digitalDisplayService.SetDisplayStatusAsync(display.Id,
                            oex.Message);
                        return;
                    }

                    // get the sets that this display is associated with
                    var sets = await _digitalDisplayService
                        .GetDisplaysSetsAsync(new[] { display.Id });

                    if (sets.Count == 0)
                    {
                        // if the display is not associated with any sets, clear all the slides
                        foreach (var currentSlide in currentSlides)
                        {
                            try
                            {
                                _logger.LogInformation("Display {DisplayId} named {Name} deleting slide {SlideName}",
                                    display.Id,
                                    display.Name,
                                    currentSlide.Name);
                                await _screenlyService
                                    .RemoveSlideAsync(display, currentSlide.AssetId);
                                deleted++;
                            }
                            catch (OcudaException oex)
                            {
                                _logger.LogError("Unable to delete - display {DisplayId} named {Name} asset {AssetId}: {ErrorMessage}",
                                    display.Id,
                                    display.Name,
                                    currentSlide.AssetId,
                                    oex.Message);
                            }
                        }
                    }
                    else
                    {
                        // assets which should be in the display
                        var assets = await _digitalDisplayAssetSetRepository
                            .GetAssetsBySetsAsync(sets.Select(_ => _.DigitalDisplaySetId));

                        // our record of assets in the display now
                        var displayItems = await _digitalDisplayItemRepository
                            .GetByDisplayAsync(display.Id);

                        // loop through assets that should be in this display
                        foreach (var asset in assets)
                        {
                            // look up asset id for if we think it's present (is in displayitems)
                            var assetInDisplay = displayItems.SingleOrDefault(_ =>
                                _.DigitalDisplayAssetId == asset.DigitalDisplayAssetId);

                            if (assetInDisplay == null)
                            {
                                // asset is not present, upload it, update displayItems
                                try
                                {
                                    string filePath = _digitalDisplayService
                                        .GetAssetPath(asset.DigitalDisplayAsset.Path);
                                    _logger.LogInformation("Display {DisplayId} named {Name} uploading slide {AssetId}",
                                        display.Id,
                                        display.Name,
                                        asset.DigitalDisplayAssetId);
                                    var newItem = new DigitalDisplayItem
                                    {
                                        AssetId = await _screenlyService
                                            .AddSlideAsync(display, filePath, asset),
                                        DigitalDisplayAssetId = asset.DigitalDisplayAssetId,
                                        DigitalDisplayId = display.Id
                                    };
                                    await _digitalDisplayItemRepository.AddAsync(newItem);
                                    displayItems.Add(newItem);
                                    uploaded++;
                                }
                                catch (OcudaException oex)
                                {
                                    _logger.LogError(oex,
                                        "Unable to add - display {DisplayId} named {Name} asset {AssetId}: {ErrorMessage}",
                                        display.Id,
                                        display.Name,
                                        asset.DigitalDisplayAssetId,
                                        oex.Message);
                                }
                            }
                            else
                            {
                                // asset is present in displayitems so we think it's in the display
                                // check based on what we got from the api
                                var currentSlide = currentSlides
                                    .SingleOrDefault(_ => _.AssetId == assetInDisplay.AssetId);

                                if (currentSlide == null)
                                {
                                    // we think it's there (in displayitems) but it's not
                                    // upload it and update the displayitem record with the new assetid
                                    try
                                    {
                                        string filePath = _digitalDisplayService
                                            .GetAssetPath(asset.DigitalDisplayAsset.Path);
                                        _logger.LogInformation("Display {DisplayId} named {Name} uploading missing slide {AssetId}",
                                            display.Id,
                                            display.Name,
                                            asset.DigitalDisplayAssetId);
                                        displayItems.Remove(assetInDisplay);
                                        assetInDisplay.AssetId = await _screenlyService
                                            .AddSlideAsync(display, filePath, asset);
                                        _digitalDisplayItemRepository.Update(assetInDisplay);
                                        displayItems.Add(assetInDisplay);
                                        uploaded++;
                                    }
                                    catch (OcudaException oex)
                                    {
                                        _logger.LogError(oex,
                                            "Unable to add - display {DisplayId} named {Name} asset {AssetId}: {ErrorMessage}",
                                            display.Id,
                                            display.Name,
                                            asset.DigitalDisplayAssetId,
                                            oex.Message);
                                    }
                                }
                                else
                                {
                                    // in displayitems, in according to the api, verify the metadata
                                    var setCorrectly = asset.StartDate == currentSlide.StartDate
                                        && asset.EndDate == currentSlide.EndDate
                                        && asset.IsEnabled == (currentSlide.IsEnabled == 1);

                                    if (!setCorrectly)
                                    {
                                        // if there's a metadata error, update metadata via the api
                                        try
                                        {
                                            _logger.LogInformation("Display {DisplayId} named {Name} updating asset {AssetId}",
                                                display.Id,
                                                display.Name,
                                                asset.DigitalDisplayAssetId);
                                            await _screenlyService.UpdateSlideAsync(display,
                                                currentSlide.AssetId,
                                                asset);
                                            updated++;
                                        }
                                        catch (OcudaException oex)
                                        {
                                            _logger.LogError("Unable to update - display {DisplayId} named {Name} asset {AssetId}: {ErrorMessage}",
                                                display.Id,
                                                display.Name,
                                                currentSlide.AssetId,
                                                oex.Message);
                                        }
                                    }
                                }
                            }
                        }

                        // now check for items in displayitems that are not in assets
                        // i.e. we think they're on the screen but they aren't in an associated set
                        var validAssetIds = assets.Select(_ => _.DigitalDisplayAssetId);
                        var staleDisplayItems = displayItems
                            .Where(_ => !validAssetIds.Contains(_.DigitalDisplayAssetId))
                            .ToList();

                        // remove those items from displayitems
                        foreach (var staleDisplayItem in staleDisplayItems)
                        {
                            _digitalDisplayItemRepository
                                .RemoveByAssetId(staleDisplayItem.DigitalDisplayAssetId);
                            displayItems.Remove(staleDisplayItem);
                        }

                        // look for slides via the api that are not in the displayitems list
                        var assetIdsToRemove = currentSlides.Select(_ => _.AssetId)
                            .Except(displayItems.Select(_ => _.AssetId));

                        // remove these slides from the display
                        foreach (var assetId in assetIdsToRemove)
                        {
                            try
                            {
                                _logger.LogInformation("Display {DisplayId} named {Name} deleting slide {SlideName}",
                                    display.Id,
                                    display.Name,
                                    currentSlides.Single(_ => _.AssetId == assetId).Name);
                                await _screenlyService.RemoveSlideAsync(display, assetId);
                                deleted++;
                            }
                            catch (OcudaException oex)
                            {
                                _logger.LogError("Unable to delete - display {DisplayId} named {Name} asset {AssetId}: {ErrorMessage}",
                                    display.Id,
                                    display.Name,
                                    assetId,
                                    oex.Message);
                            }
                        }
                    }

                    _logger.LogDebug("Updated display id {DisplayId} named {Name} slides: {StartedSlides} +{UploadedSlides}, -{DeletedSlides}, updated {UpdatedSlides}",
                        display.Id,
                        display.Name,
                        startingSlides,
                        uploaded,
                        deleted,
                        updated);

                    await _digitalDisplayService.SetDisplayStatusAsync(display.Id,
                        $"Updated: added {uploaded}, updated {updated}, deleted {deleted}");

                    await _digitalDisplayRepository
                        .UpdateLastVerificationAsync(display.Id, DateTime.Now);
                }
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex,
                    "Uncaught error syncing displays: {ErrorMessage}",
                    oex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Uncaught critical error syncing displays: {ErrorMessage}",
                    ex.Message);
            }
        }

        private async Task<DigitalDisplay> GetPendingDisplayAsync()
        {
            var displays = await _digitalDisplayRepository.GetAllAsync();

            var lastAttempt = displays.OrderBy(_ => _.LastAttempt).FirstOrDefault();

            if (lastAttempt != null)
            {
                _logger.LogDebug("Syncing display id {DisplayId} named {Name} (last attempted at {LastAttempt:s})",
                    lastAttempt.Id,
                    lastAttempt.Name,
                    lastAttempt.LastAttempt);
            }

            return lastAttempt;
        }
    }
}