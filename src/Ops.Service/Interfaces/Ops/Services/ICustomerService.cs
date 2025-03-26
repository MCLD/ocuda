using System.Collections.Generic;
using System.Threading.Tasks;
using BooksByMail.Models;
using Ocuda.Ops.Service.Filters;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ICustomerService
    {
        Task<DataWithCount<List<Customer>>> GetPaginatedPatronListAsync(PolarisPatronFilter filter);
        Task<Customer> GetPatronInfoAsync(int patronID);
        Task<List<Material>> GetPatronCheckoutsAsync(int patronID);
        Task<int> GetPatronHistoryCountAsync(int patronID);
        Task<DataWithCount<List<Material>>> GetPaginatedPatronHistoryAsync(PolarisItemFilter filter);
        Task<List<Material>> GetPatronHoldsAsync(int patronID);
    }
}