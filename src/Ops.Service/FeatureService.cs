using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class FeatureService : BaseService<FeatureService>, IFeatureService
    {
        private readonly IFeatureRepository _featureRepository;

        public FeatureService(ILogger<FeatureService> logger,
            IHttpContextAccessor httpContextAccessor,
            IFeatureRepository featureRepository)
            : base(logger, httpContextAccessor)
        {
            _featureRepository = featureRepository
                ?? throw new ArgumentNullException(nameof(featureRepository));
        }

        public async Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _featureRepository.GetPaginatedListAsync(filter);
        }

        public async Task<List<Feature>> GetAllFeaturesAsync()
        {
            return await _featureRepository.GetAllFeaturesAsync();
        }

        public async Task<Feature> GetFeatureByNameAsync(string featureName)
        {
            try
            {
                return await _featureRepository.GetFeatureByName(featureName);
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex, "Problem finding feature: {Message}", ex.Message);
                throw new OcudaException($"Could not find feature: {featureName}");
            }
        }

        public async Task<Feature> GetFeatureByIdAsync(int featureId)
        {
            return await _featureRepository.FindAsync(featureId);
        }

        public async Task<Feature> AddFeatureAsync(Feature feature)
        {
            try
            {
                feature.FontAwesome = "fa-inverse " + feature.FontAwesome + " fa-stack-1x";
                if (!feature.FontAwesome.Contains("fab"))
                {
                    feature.FontAwesome = "fa " + feature.FontAwesome;
                }
                feature.Name = feature.Name?.Trim();
                feature.BodyText = feature.BodyText?.Trim();
                feature.Stub = feature.Stub?.Trim();
                feature.CreatedAt = DateTime.Now;
                feature.CreatedBy = GetCurrentUserId();

                await ValidateAsync(feature);
                await _featureRepository.AddAsync(feature);
                await _featureRepository.SaveAsync();
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            return feature;
        }

        public async Task<Feature> EditAsync(Feature feature)
        {
            var currentFeature = await _featureRepository.FindAsync(feature.Id);
            await ValidateAsync(currentFeature);
            if (currentFeature != null)
            {
                try
                {
                    if (!feature.FontAwesome.Contains("fa-inverse"))
                    {
                        feature.FontAwesome = "fa-inverse " + feature.FontAwesome + " fa-stack-1x";
                        if (!feature.FontAwesome.Contains("fab"))
                        {
                            feature.FontAwesome = "fa " + feature.FontAwesome;
                        }
                    }
                    currentFeature.BodyText = feature.BodyText?.Trim();
                    currentFeature.FontAwesome = feature.FontAwesome;
                    currentFeature.Name = feature.Name?.Trim();
                    currentFeature.Stub = feature.Stub?.Trim();
                    currentFeature.UpdatedAt = DateTime.Now;
                    currentFeature.UpdatedBy = GetCurrentUserId();

                    _featureRepository.Update(feature);
                    await _featureRepository.SaveAsync();
                    return await _featureRepository.FindAsync(currentFeature.Id);
                }
                catch (OcudaException ex)
                {
                    _logger.LogError(ex.Message);
                    throw;
                }
            }
            else
            {
                throw new OcudaException($"Could not find feature id {feature.Id} to edit.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            _featureRepository.Remove(id);
            await _featureRepository.SaveAsync();
        }

        public async Task<DataWithCount<ICollection<Feature>>> PageItemsAsync(
            FeatureFilter filter)
        {
            return new DataWithCount<ICollection<Feature>>
            {
                Data = await _featureRepository.PageAsync(filter),
                Count = await _featureRepository.CountAsync(filter)
            };
        }

        private async Task ValidateAsync(Feature feature)
        {
            if (await _featureRepository.IsDuplicateNameAsync(feature))
            {
                throw new OcudaException($"A feature named '{feature.Name}' already exists.");
            }
            if (!string.IsNullOrEmpty(feature.Stub)
                && await _featureRepository.IsDuplicateStubAsync(feature))
            {
                throw new OcudaException($"A feature with stub '{feature.Stub}' already exists.");
            }
        }
    }
}