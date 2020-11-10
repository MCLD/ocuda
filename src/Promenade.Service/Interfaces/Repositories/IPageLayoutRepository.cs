using System;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPageLayoutRepository : IGenericRepository<PageLayout>
    {
        Task<int?> GetCurrentLayoutIdForHeaderAsync(int headerId);
        Task<PageLayout> GetIncludingChildrenAsync(int id);
        Task<int?> GetPreviewLayoutIdAsync(int headerId, Guid previewId);
    }
}
