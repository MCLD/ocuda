using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class FeatureService : BaseService<FeatureService>, IFeatureService
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly ILocationFeatureService _locationFeatureService;
        private readonly ILocationService _locationService;

        public FeatureService(ILogger<FeatureService> logger,
            IHttpContextAccessor httpContextAccessor,
            IFeatureRepository featureRepository,
            ILocationService locationService,
            ILocationFeatureService locationFeatureService) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(featureRepository);
            ArgumentNullException.ThrowIfNull(locationFeatureService);
            ArgumentNullException.ThrowIfNull(locationService);

            _featureRepository = featureRepository;
            _locationFeatureService = locationFeatureService;
            _locationService = locationService;
        }

        public async Task<Feature> AddFeatureAsync(Feature feature)
        {
            ArgumentNullException.ThrowIfNull(feature);

            feature.Name = feature.Name?.Trim();
            feature.Stub = feature.Stub?.Trim();

            await ValidateAsync(feature);
            await _featureRepository.AddAsync(feature);
            await _featureRepository.SaveAsync();

            return feature;
        }

        public async Task DeleteAsync(int id)
        {
            var locationIds = await _locationFeatureService.GetLocationsByFeatureIdAsync(id);

            if (locationIds?.Any() == true)
            {
                var locations = await _locationService.GetAllLocationsIdNameAsync();

                var subset = locations.Where(_ => locationIds.Contains(_.Key)).Select(_ => _.Value);

                throw new OcudaException($"That feature is in use at the folowing locations: {string.Join(", ", subset)}");
            }

            var feature = await _featureRepository.FindAsync(id);
            _featureRepository.Remove(feature);
            await _featureRepository.SaveAsync();
        }

        public async Task<Feature> EditAsync(Feature feature)
        {
            ArgumentNullException.ThrowIfNull(feature);

            var currentFeature = await _featureRepository.FindAsync(feature.Id);
            if (currentFeature != null)
            {
                currentFeature.BodyText = feature.BodyText?.Trim();
                currentFeature.Icon = feature.Icon;
                currentFeature.Name = feature.Name?.Trim();
                currentFeature.Stub = feature.Stub?.Trim() ?? currentFeature.Stub;
                currentFeature.IsAtThisLocation = feature.IsAtThisLocation;
                currentFeature.NameSegmentId = feature.NameSegmentId;
                currentFeature.TextSegmentId = feature.TextSegmentId;

                _featureRepository.Update(feature);
                await _featureRepository.SaveAsync();
                return await _featureRepository.FindAsync(currentFeature.Id);
            }
            else
            {
                throw new OcudaException($"Could not find feature id {feature.Id} to edit.");
            }
        }

        public async Task<List<Feature>> GetAllFeaturesAsync()
        {
            return await _featureRepository.GetAllFeaturesAsync();
        }

        public async Task<Feature> GetFeatureByIdAsync(int featureId)
        {
            return await _featureRepository.FindAsync(featureId);
        }

        public async Task<Feature> GetFeatureBySegmentIdAsync(int segmentId)
        {
            return await _featureRepository.GetBySegmentIdAsync(segmentId);
        }

        public async Task<Feature> GetFeatureBySlugAsync(string slug)
        {
            return await _featureRepository.GetBySlugAsync(slug);
        }

        public async Task<ICollection<Feature>> GetFeaturesByIdsAsync(IEnumerable<int> featureIds)
        {
            return await _featureRepository.GetByIdsAsync(featureIds);
        }

        public async Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _featureRepository.GetPaginatedListAsync(filter);
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

        public async Task UpdateFeatureNameAsync(int featureId, string newName)
        {
            if (!string.IsNullOrEmpty(newName))
            {
                await _featureRepository.UpdateName(featureId, newName.Trim());
            }
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