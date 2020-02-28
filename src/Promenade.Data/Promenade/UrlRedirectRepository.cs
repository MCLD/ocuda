﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class UrlRedirectRepository
        : GenericRepository<PromenadeContext, UrlRedirect>, IUrlRedirectRepository
    {
        public UrlRedirectRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<UrlRedirectRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<UrlRedirect> GetRedirectIdPathAsync(string path)
        {
            var redirect = await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.RequestPath == path)
                .Select(_ => new
                {
                    _.Id,
                    _.Url,
                })
                .SingleOrDefaultAsync();

            if (redirect == null)
            {
                return null;
            }

            return new UrlRedirect
            {
                Id = redirect.Id,
                Url = redirect.Url
            };
        }
    }
}
