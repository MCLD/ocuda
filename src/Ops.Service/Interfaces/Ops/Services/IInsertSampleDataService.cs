using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IInsertSampleDataService
    {
        Task InsertDataAsync();
        Task<ICollection<Section>> InsertSectionsAsync();
        Task InsertUserMetadataTypesAsync();
        Task InsertUsersAsync();
        Task<ICollection<FileType>> InsertFileTypesAsync();
        Task InsertPagesAsync(int sectionId);
        Task InsertFileLibrariesAsync(Section section, ICollection<FileType> fileTypes);
        Task InsertPostsAsync(int categoryId);
        Task<ICollection<LinkLibrary>> InsertLinkLibrariesAsync(Section section);
        Task InsertLinksAsync(int libraryId);
    }
}
