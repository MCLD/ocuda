using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class CustomerLookupService : ICustomerLookupService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerLookupService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        public Task<DataWithCount<List<Customer>>> GetPaginatedPatronListAsync(CustomerLookupFilter filter)
        {
            return _customerRepository.GetPaginatedPatronListAsync(filter);
        }

        public Task<Customer> GetPatronInfoAsync(int patronID)
        {
            return _customerRepository.GetPatronInfoAsync(patronID);
        }

        public Task<List<Material>> GetPatronCheckoutsAsync(int patronID)
        {
            return _customerRepository.GetPatronCheckoutsAsync(patronID);
        }

        public Task<int> GetPatronHistoryCountAsync(int patronID)
        {
            return _customerRepository.GetPatronHistoryCountAsync(patronID);
        }

        public Task<DataWithCount<List<Material>>> GetPaginatedPatronHistoryAsync(MaterialFilter filter)
        {
            return _customerRepository.GetPaginatedPatronHistoryAsync(filter);
        }

        public Task<List<Material>> GetPatronHoldsAsync(int patronID)
        {
            return _customerRepository.GetPatronHoldsAsync(patronID);
        }
    }
}

