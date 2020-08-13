﻿using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPodcastRepository : IGenericRepository<Podcast>
    {
        Task<Podcast> GetByStubAsync(string stub);
    }
}
