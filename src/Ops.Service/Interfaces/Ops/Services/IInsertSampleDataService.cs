using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IInsertSampleDataService
    {
        Task InsertDataAsync();

        Task InsertUserMetadataTypesAsync();

        Task InsertUsersAsync();

        Task<ICollection<FileType>> InsertFileTypesAsync();
    }
}