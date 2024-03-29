﻿using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationFormRepository : IGenericRepository<LocationForm>
    {
        Task<LocationForm> FindAsync(int formId, int locationId);
    }
}