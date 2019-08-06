using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPageRepository
    {
        Task<Page> GetByStubAndTypeAsync(string stub, PageType type);
    }
}
