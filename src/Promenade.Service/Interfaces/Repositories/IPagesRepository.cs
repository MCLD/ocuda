using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPageRepository
    {
        Task<Page> GetPublishedByStubAndTypeAsync(string stub, PageType type, int languageId);
    }
}
