using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
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

        public Task<DataWithCount<IList<Customer>>> GetPaginatedCustomerLookupListAsync(CustomerLookupFilter filter)
        {
            return _customerRepository.GetPaginatedCustomerLookupListAsync(filter);
        }

        public Task<Customer> GetCustomerLookupInfoAsync(int customerLookupID)
        {
            return _customerRepository.GetCustomerLookupInfoAsync(customerLookupID);
        }

        public Task<IList<Material>> GetCustomerLookupCheckoutsAsync(int customerLookupID)
        {
            return _customerRepository.GetCustomerLookupCheckoutsAsync(customerLookupID);
        }

        public Task<int> GetCustomerLookupHistoryCountAsync(int customerLookupID)
        {
            return _customerRepository.GetCustomerLookupHistoryCountAsync(customerLookupID);
        }

        public Task<DataWithCount<IList<Material>>> GetPaginatedCustomerLookupHistoryAsync(MaterialFilter filter)
        {
            return _customerRepository.GetPaginatedCustomerLookupHistoryAsync(filter);
        }

        public Task<IList<Material>> GetCustomerLookupHoldsAsync(int customerLookupID)
        {
            return _customerRepository.GetCustomerLookupHoldsAsync(customerLookupID);
        }
    }
}