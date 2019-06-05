using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IPageService
    {
        Task<int> GetPageCountAsync();
        Task<ICollection<Page>> GetPagesAsync(int skip = 0, int take = 5);
        Task<Page> GetByIdAsync(int id);
        Task<Page> GetByStubAsync(string stub);
        Task<Page> GetByStubAndSectionIdAsync(string stub, int sectionId);
        Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BlogFilter filter);
        Task<Page> CreateAsync(int currentUserId, Page page);
        Task<Page> EditAsync(Page page, bool publish = false);
        Task DeleteAsync(int id);
        Task<bool> StubInUseAsync(Page page);
    }
}
