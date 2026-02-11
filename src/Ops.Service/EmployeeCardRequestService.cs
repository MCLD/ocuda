using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class EmployeeCardRequestService : BaseService<EmployeeCardRequestService>,
        IEmployeeCardRequestService
    {
        private readonly IOcudaCache _cache;
        private readonly IEmployeeCardDepartmentRepository _employeeCardDepartmentRepository;
        private readonly IEmployeeCardRequestRepository _employeeCardRequestRepository;

        public EmployeeCardRequestService(ILogger<EmployeeCardRequestService> logger,
            IHttpContextAccessor httpContext,
            IOcudaCache cache,
            IEmployeeCardDepartmentRepository employeeCardDepartmentRepository,
            IEmployeeCardRequestRepository employeeCardRequestRepository)
            : base(logger, httpContext)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(employeeCardDepartmentRepository);
            ArgumentNullException.ThrowIfNull(employeeCardRequestRepository);

            _cache = cache;
            _employeeCardDepartmentRepository = employeeCardDepartmentRepository;
            _employeeCardRequestRepository = employeeCardRequestRepository;
        }

        public async Task<string> GetDepartmentNameAsync(int departmentId)
        {
            var cacheKey = string.Format(
                    CultureInfo.InvariantCulture,
                    Cache.OpsEmployeeCardDepartmentName,
                    departmentId);

            var departmentName = await _cache.GetStringFromCache(cacheKey);

            if (string.IsNullOrWhiteSpace(departmentName))
            {
                departmentName = await _employeeCardDepartmentRepository
                    .GetDepartmentNameAsync(departmentId);
                await _cache.SaveToCacheAsync(cacheKey, departmentName, 24);
            }

            return departmentName;
        }

        public async Task<EmployeeCardRequest> GetRequestAsync(int id)
        {
            return await _employeeCardRequestRepository.GetByIdAsync(id);
        }


        public async Task<int> GetRequestCountAsync()
        {
            return await _employeeCardRequestRepository.CountAsync();
        }

        public async Task<CollectionWithCount<EmployeeCardRequest>> GetRequestsAsync(
            BaseFilter filter)
        {
            return await _employeeCardRequestRepository.GetPaginatedAsync(filter);
        }
    }
}
