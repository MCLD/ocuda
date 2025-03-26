using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BooksByMail.Models;
using Ocuda.Ops.Service.Filters;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICustomerRepository
    {
        Task<DataWithCount<List<Customer>>> GetPaginatedPatronListAsync(PolarisPatronFilter filter);
        Task<Customer> GetPatronInfoAsync(int patronID);
        Task<List<Material>> GetPatronCheckoutsAsync(int patronID);
        Task<int> GetPatronHistoryCountAsync(int patronID);
        Task<DataWithCount<List<Material>>> GetPaginatedPatronHistoryAsync(PolarisItemFilter filter);
        Task<List<Material>> GetPatronHoldsAsync(int patronID);
    }
}
