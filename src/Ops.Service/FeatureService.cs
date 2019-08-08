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
    public class FeatureService : IFeatureService
    {
        private readonly ILogger<FeatureService> _logger;
        private readonly IFeatureRepository _featureRepository;

        public FeatureService(ILogger<FeatureService> logger,
            IFeatureRepository featureRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _featureRepository = featureRepository
                ?? throw new ArgumentNullException(nameof(featureRepository));
        }

        public async Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _featureRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Feature> GetFeatureByNameAsync(string featureName)
        {
            try
            {
                return await _featureRepository.GetFeatureByName(featureName);
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex.Message);
                throw new OcudaException($"The feature: " + featureName + " was unable to be found.");
            }
        }

        public async Task<Feature> AddFeatureAsync(Feature feature)
        {
            try
            {
                feature.Name = feature.Name?.Trim();
                await ValidateAsync(feature);

                await _featureRepository.AddAsync(feature);
                await _featureRepository.SaveAsync();
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex.Message);
                throw new OcudaException(ex.Message);
            }

            return feature;
        }

        public async Task<Feature> EditAsync(Feature feature)
        {
            var currentFeature = await _featureRepository.FindAsync(feature.Id);
            if(currentFeature != null)
            {
                try
                {
                    currentFeature.BodyText = feature.BodyText;
                    currentFeature.FontAwesome = feature.FontAwesome;
                    currentFeature.Name = feature.Name;
                    currentFeature.Stub = feature.Stub;
                    _featureRepository.Update(feature);
                    await _featureRepository.SaveAsync();
                    return await _featureRepository.FindAsync(currentFeature.Id);
                }
                catch (OcudaException ex)
                {
                    _logger.LogError(ex.Message);
                    throw new OcudaException(ex.Message);
                }
            }
            else
            {
                throw new OcudaException($"Could not find Feature.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _featureRepository.Remove(id);
                await _featureRepository.SaveAsync();
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex.Message);
                throw new OcudaException(ex.Message);
            }
        }

        private async Task ValidateAsync(Feature feature)
        {
            if (await _featureRepository.IsDuplicateAsync(feature))
            {
                throw new OcudaException($"Feature type '{feature.Name}' already exists.");
            }
        }
    }
}