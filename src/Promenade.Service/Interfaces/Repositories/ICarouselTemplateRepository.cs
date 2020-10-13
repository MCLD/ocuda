﻿using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICarouselTemplateRepository : IGenericRepository<CarouselTemplate>
    {
        Task<CarouselTemplate> GetTemplateForCarouselAsync(int carouselId);
    }
}
