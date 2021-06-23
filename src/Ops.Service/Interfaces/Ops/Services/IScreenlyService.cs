using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Models.Screenly.v11;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IScreenlyService
    {
        public Task<string> AddSlideAsync(DigitalDisplay display,
            string filePath,
            DigitalDisplayAssetSet assetSet);

        public Task<ICollection<AssetModel>> GetCurrentSlidesAsync(DigitalDisplay display);

        public Task<string> RemoveSlideAsync(DigitalDisplay display,
            string assetId);

        public Task UpdateSlideAsync(DigitalDisplay display,
            string assetId,
            DigitalDisplayAssetSet assetSet);
    }
}