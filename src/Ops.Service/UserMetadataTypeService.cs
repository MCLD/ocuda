using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class UserMetadataTypeService
        : BaseService<UserMetadataTypeService>, IUserMetadataTypeService
    {
        private readonly IUserMetadataTypeRepository _userMetadataTypeRepository;

        public UserMetadataTypeService(ILogger<UserMetadataTypeService> logger,
            IHttpContextAccessor httpContextAccessor,
            IUserMetadataTypeRepository userMetadataTypeRepository)
            : base(logger, httpContextAccessor)
        {
            _userMetadataTypeRepository = userMetadataTypeRepository
                ?? throw new ArgumentNullException(nameof(userMetadataTypeRepository));
        }

        public async Task<ICollection<UserMetadataType>> GetAllAsync()
        {
            return await _userMetadataTypeRepository.GetAllAsync();
        }

        public async Task<DataWithCount<ICollection<UserMetadataType>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _userMetadataTypeRepository.GetPaginatedListAsync(filter);
        }

        public async Task<UserMetadataType> AddAsync(UserMetadataType metadataType)
        {
            metadataType.CreatedAt = DateTime.Now;
            metadataType.CreatedBy = GetCurrentUserId();
            metadataType.Name = metadataType.Name?.Trim();

            await ValidateAsync(metadataType);

            await _userMetadataTypeRepository.AddAsync(metadataType);
            await _userMetadataTypeRepository.SaveAsync();

            return metadataType;
        }

        public async Task<UserMetadataType> EditAsync(UserMetadataType metadataType)
        {
            var currentMetadataType = await _userMetadataTypeRepository.FindAsync(metadataType.Id);

            currentMetadataType.Name = metadataType.Name?.Trim();
            currentMetadataType.IsPublic = metadataType.IsPublic;

            await ValidateAsync(currentMetadataType);

            _userMetadataTypeRepository.Update(currentMetadataType);
            await _userMetadataTypeRepository.SaveAsync();

            return currentMetadataType;
        }

        public async Task DeleteAsync(int id)
        {
            _userMetadataTypeRepository.Remove(id);
            await _userMetadataTypeRepository.SaveAsync();
        }

        private async Task ValidateAsync(UserMetadataType metadataType)
        {
            if (await _userMetadataTypeRepository.IsDuplicateAsync(metadataType))
            {
                throw new OcudaException($"Metdata type '{metadataType.Name}' already exists.");
            }
        }
    }
}
