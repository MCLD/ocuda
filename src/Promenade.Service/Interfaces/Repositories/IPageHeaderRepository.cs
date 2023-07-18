using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPageHeaderRepository : IGenericRepository<PageHeader>
    {
        Task<PageHeader> GetByIdAndTypeAsync(int id, PageType type);

        Task<PageHeader> GetByStubAndTypeAsync(string stub, PageType type);
    }
}