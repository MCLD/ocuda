using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IEmployeeCardService
    {
        Task<EmployeeCardNote> GetRequestNoteAsync(int requestId);
        Task<EmployeeCardResult> GetResultAsync(int id);
        Task<int> GetResultCountAsync();
        Task<CollectionWithCount<EmployeeCardResult>> GetResultsAsync(BaseFilter filter);
        Task<EmployeeCardResult> ProcessRequestAsync(int requestId,
            string cardNumber,
            EmployeeCardResult.ResultType type);
        Task SetRequestNote(EmployeeCardNote note);
    }
}
