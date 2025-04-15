using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;
using Ocuda.Ops.Service.Filters;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICustomerRepository
    {
        Task<DataWithCount<List<Customer>>> GetPaginatedPatronListAsync(CustomerLookupFilter filter);
        Task<Customer> GetPatronInfoAsync(int patronID);
        Task<List<Material>> GetPatronCheckoutsAsync(int patronID);
        Task<int> GetPatronHistoryCountAsync(int patronID);
        Task<DataWithCount<List<Material>>> GetPaginatedPatronHistoryAsync(MaterialFilter filter);
        Task<List<Material>> GetPatronHoldsAsync(int patronID);
    }
}
