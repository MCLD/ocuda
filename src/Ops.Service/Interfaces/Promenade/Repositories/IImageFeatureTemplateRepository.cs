﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IImageFeatureTemplateRepository : IGenericRepository<ImageFeatureTemplate>
    {
        Task<ICollection<ImageFeatureTemplate>> GetAllAsync();

        Task<ImageFeatureTemplate> GetForImageFeatureAsync(int imageFeatureId);
    }
}