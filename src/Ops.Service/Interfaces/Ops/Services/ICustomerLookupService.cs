using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;
using Ocuda.Ops.Service.Filters;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ICustomerLookupService
    {
        Task<DataWithCount<List<Customer>>> GetPaginatedCustomerLookupListAsync(CustomerLookupFilter filter);

        Task<Customer> GetCustomerLookupInfoAsync(int customerLookupID);

        Task<List<Material>> GetCustomerLookupCheckoutsAsync(int customerLookupID);

        Task<int> GetCustomerLookupHistoryCountAsync(int customerLookupID);

        Task<DataWithCount<List<Material>>> GetPaginatedCustomerLookupHistoryAsync(MaterialFilter filter);

        Task<List<Material>> GetCustomerLookupHoldsAsync(int customerLookupID);
    }
}