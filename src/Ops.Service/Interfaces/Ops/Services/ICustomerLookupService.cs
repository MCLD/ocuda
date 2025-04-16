using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ICustomerLookupService
    {
        Task<DataWithCount<List<Customer>>> GetPaginatedPatronListAsync(CustomerLookupFilter filter);

        Task<Customer> GetPatronInfoAsync(int patronID);

        Task<List<Material>> GetPatronCheckoutsAsync(int patronID);

        Task<int> GetPatronHistoryCountAsync(int patronID);

        Task<DataWithCount<List<Material>>> GetPaginatedPatronHistoryAsync(MaterialFilter filter);

        Task<List<Material>> GetPatronHoldsAsync(int patronID);
    }
}