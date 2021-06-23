﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IDigitalDisplayAssetRepository : IOpsRepository<DigitalDisplayAsset, int>
    {
        public Task<DigitalDisplayAsset> FindByChecksumAsync(byte[] checksum);

        public Task<DataWithCount<ICollection<DigitalDisplayAsset>>>
            GetPaginatedListAsync(BaseFilter filter);
    }
}