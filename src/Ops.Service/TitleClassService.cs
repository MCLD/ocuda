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
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class TitleClassService : BaseService<TitleClassService>, ITitleClassService
    {
        private readonly ITitleClassRepository _titleClassRepository;

        public TitleClassService(ILogger<TitleClassService> logger,
            IHttpContextAccessor httpContextAccessor,
            ITitleClassRepository titleClassRepository)
            : base(logger, httpContextAccessor)
        {
            _titleClassRepository = titleClassRepository
                ?? throw new ArgumentNullException(nameof(titleClassRepository));
        }

        public async Task AddTitleAsync(int titleClassId, string title)
        {
            await _titleClassRepository.AddTitleAsync(GetCurrentUserId(), titleClassId, title);
        }

        public async Task<TitleClass> GetAsync(int titleClassId)
        {
            return await _titleClassRepository.FindAsync(titleClassId);
        }

        public async Task<CollectionWithCount<TitleClass>> GetPaginatedAsync(BaseFilter filter)
        {
            return await _titleClassRepository.GetPaginatedAsync(filter);
        }

        public async Task<IEnumerable<TitleClass>> GetTitleClassByTitleAsync(string title)
        {
            return await _titleClassRepository.GetByTitleAsync(title);
        }

        public async Task<int> NewTitleClassificationAsync(string titleClassName, string title)
        {
            return await _titleClassRepository
                .NewTitleClassificationAsync(GetCurrentUserId(), titleClassName, title);
        }

        public async Task<bool> RemoveTitleAsync(int titleClassId, string title)
        {
            return await _titleClassRepository.RemoveTitleAsync(titleClassId, title);
        }
    }
}
