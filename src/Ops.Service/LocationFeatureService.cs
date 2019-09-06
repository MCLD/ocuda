using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class LocationFeatureService : ILocationFeatureService
    {
        private readonly ILocationFeatureRepository _locationFeatureRepository;

        public LocationFeatureService(ILocationFeatureRepository locationFeatureRepository)
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

        public async Task<LocationFeature> GetLocationFeatureByIdAsync(int locationFeatureId)
        {
            return await _locationFeatureRepository.FindAsync(locationFeatureId);
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
            var currentLocationFeature = await _locationFeatureRepository.FindAsync(locationFeature.Id);
            currentLocationFeature.Text = locationFeature.Text;
            currentLocationFeature.RedirectUrl = locationFeature.RedirectUrl;
            await ValidateAsync(currentLocationFeature);
            _locationFeatureRepository.Update(currentLocationFeature);
            await _locationFeatureRepository.SaveAsync();
            return currentLocationFeature;
        }

        public async Task DeleteAsync(int id)
        {
            _locationFeatureRepository.Remove(id);
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
