using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class LocationFeatureService : BaseService<LocationFeatureService>, ILocationFeatureService
    {
        private readonly ILocationFeatureRepository _locationFeatureRepository;

        public LocationFeatureService(ILogger<LocationFeatureService> logger,
            IHttpContextAccessor httpContextAccessor,
            ILocationFeatureRepository locationFeatureRepository)
            : base(logger, httpContextAccessor)
        {
            _locationFeatureRepository = locationFeatureRepository
                ?? throw new ArgumentNullException(nameof(locationFeatureRepository));
        }

        public async Task<List<LocationFeature>> GetAllLocationFeaturesAsync()
        {
            return await _locationFeatureRepository.GeAllLocationFeaturesAsync();
        }

        public async Task<List<LocationFeature>> GetLocationFeaturesByLocationAsync(Location location)
        {
            return await _locationFeatureRepository.GetLocationFeaturesByLocationId(location.Id);
        }

        public async Task<LocationFeature> GetByIdsAsync(int featureId, int locationId)
        {
            return await _locationFeatureRepository.GetByIdsAsync(featureId, locationId);
        }

        public async Task<LocationFeature> AddLocationFeatureAsync(LocationFeature locationFeature)
        {
            await ValidateAsync(locationFeature);

            await _locationFeatureRepository.AddAsync(locationFeature);
            await _locationFeatureRepository.SaveAsync();
            return locationFeature;
        }

        public async Task<LocationFeature> EditAsync(LocationFeature locationFeature)
        {
            var currentLocationFeature = await _locationFeatureRepository
                .GetByIdsAsync(locationFeature.FeatureId, locationFeature.LocationId);
            currentLocationFeature.Text = locationFeature.Text;
            currentLocationFeature.RedirectUrl = locationFeature.RedirectUrl;
            currentLocationFeature.NewTab = locationFeature.NewTab;

            _locationFeatureRepository.Update(currentLocationFeature);
            await _locationFeatureRepository.SaveAsync();
            return currentLocationFeature;
        }

        public async Task DeleteAsync(int featureId, int locationId)
        {
            var locationFeature = await _locationFeatureRepository
                .GetByIdsAsync(featureId, locationId);
            _locationFeatureRepository.Remove(locationFeature);
            await _locationFeatureRepository.SaveAsync();
        }

        private async Task ValidateAsync(LocationFeature locationfeature)
        {
            if (await _locationFeatureRepository.IsDuplicateAsync(locationfeature))
            {
                throw new OcudaException("Location's Feature, already exists.");
            }
        }
    }
}
