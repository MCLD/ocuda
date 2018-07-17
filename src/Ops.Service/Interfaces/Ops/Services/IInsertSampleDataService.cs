using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IInsertSampleDataService
    {
        Task InsertDataAsync();
        Task<ICollection<Section>> InsertSectionsAsync();
        Task InsertSiteSettingsAsync();
        Task InsertPostsAsync(int sectionId);
        Task<ICollection<Category>> InsertLinkCategoriesAsync(Section section);
        Task InsertLinksAsync(int sectionId);
        Task<ICollection<Category>> InsertFileCategoriesAsync(Section section);
        Task InsertFileTypesAsync();
        Task InsertPagesAsync(int sectionId);
        Task InsertUsersAsync();
    }
}
