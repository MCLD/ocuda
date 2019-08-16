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
        private readonly ILogger<LocationFeatureService> _logger;
        private readonly ILocationFeatureRepository _locationFeatureRepository;

        public LocationFeatureService(ILogger<LocationFeatureService> logger,
            ILocationFeatureRepository locationFeatureRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
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
            try
            {
                await ValidateAsync(locationFeature);
                await _locationFeatureRepository.AddAsync(locationFeature);
                await _locationFeatureRepository.SaveAsync();
            }
            catch (OcudaException ex)
            {
                throw new OcudaException(ex.Message);
            }

                return locationFeature;
        }

        public async Task<LocationFeature> EditAsync(LocationFeature locationFeature)
        {
            try
            {
                var currentLocationFeature = await _locationFeatureRepository.FindAsync(locationFeature.Id);
                currentLocationFeature.Text = locationFeature.Text;
                currentLocationFeature.RedirectUrl = locationFeature.RedirectUrl;
                await ValidateAsync(currentLocationFeature);
                _locationFeatureRepository.Update(currentLocationFeature);
                await _locationFeatureRepository.SaveAsync();
                return currentLocationFeature;
            }
            catch (OcudaException ex)
            {
                throw new OcudaException(ex.Message);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _locationFeatureRepository.Remove(id);
                await _locationFeatureRepository.SaveAsync();
            }
            catch(OcudaException ex)
            {
                throw new OcudaException(ex.Message);
            }
        }

        private async Task ValidateAsync(LocationFeature locationfeature)
        {
            if (await _locationFeatureRepository.IsDuplicateAsync(locationfeature))
            {
                throw new OcudaException($"Location Feature already exists.");
            }
        }
    }
}
