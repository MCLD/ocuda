using System.Linq;
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
            var redirects = await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.RequestPath == path)
                .Select(_ => new
                {
                    _.Id,
                    _.Url,
                })
                .ToListAsync();

            if (redirects == null || redirects.Count == 0)
            {
                return null;
            }

            if (redirects.Count > 1)
            {
                _logger.LogError("Found {RedirectsCount} redirect matches for path {Path}, returning id {Id}",
                    redirects.Count,
                    path,
                    redirects[0].Id);
            }

            return new UrlRedirect
            {
                Id = redirects[0].Id,
                Url = redirects[0].Url
            };
        }
    }
}