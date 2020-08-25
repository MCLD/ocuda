using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class UserMetadataTypeRepository
        : OpsRepository<OpsContext, UserMetadataType, int>, IUserMetadataTypeRepository
    {
        public UserMetadataTypeRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<UserMetadataTypeRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<UserMetadataType>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<UserMetadataType>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<UserMetadataType>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public override void Remove(int id)
        {
            var userMetadatas = _context.UserMetadata.Where(_ => _.UserMetadataTypeId == id);
            _context.UserMetadata.RemoveRange(userMetadatas);

            base.Remove(id);
        }

        public async Task<bool> IsDuplicateAsync(UserMetadataType metadataType)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Name.ToLower() == metadataType.Name.ToLower()
                    && _.Id != metadataType.Id)
                .AnyAsync();
        }
    }
}
