using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IDigitalDisplayItemRepository : IGenericRepository<DigitalDisplayItem>
    {
        public Task<ICollection<DigitalDisplayItem>> GetByDisplayAsync(int displayId);

        public void RemoveByAssetId(int assetId);
    }
}