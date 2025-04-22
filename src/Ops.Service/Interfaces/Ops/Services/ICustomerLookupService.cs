using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ICustomerLookupService
    {
        Task<DataWithCount<IList<CustomerLookup>>> GetPaginatedCustomerLookupListAsync(CustomerLookupFilter filter);

        Task<CustomerLookup> GetCustomerLookupInfoAsync(int customerLookupID);

        Task<IList<Material>> GetCustomerLookupCheckoutsAsync(int customerLookupID);

        Task<int> GetCustomerLookupHistoryCountAsync(int customerLookupID);

        Task<DataWithCount<IList<Material>>> GetPaginatedCustomerLookupHistoryAsync(MaterialFilter filter);

        Task<IList<Material>> GetCustomerLookupHoldsAsync(int customerLookupID);
    }
}