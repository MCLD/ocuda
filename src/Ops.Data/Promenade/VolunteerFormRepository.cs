using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class VolunteerFormRepository
        : GenericRepository<PromenadeContext, VolunteerForm>, IVolunteerFormRepository
    {
        public VolunteerFormRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<VolunteerFormRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(VolunteerForm form)
        {
            await AddAsync(form);
            await SaveAsync();
        }

        public async Task<ICollection<VolunteerForm>> FindAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.VolunteerFormType)
                .ToListAsync();
        }

        public async Task<VolunteerForm> FindAsync(int id)
        {
            return await DbSet
                .FirstOrDefaultAsync(_ => _.Id == id);
        }

        public async Task<ICollection<VolunteerForm>> FindBySegmentIdAsync(int segmentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.HeaderSegmentId == segmentId)
                .ToListAsync();
        }

        public async Task<VolunteerForm> FindByTypeAsync(VolunteerFormType type)
        {
            return await DbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.VolunteerFormType == type);
        }

        public async Task<VolunteerForm> FindByTypeAsync(int typeId)
        {
            return await DbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.VolunteerFormType == (VolunteerFormType)typeId);
        }

        public async Task<IDictionary<VolunteerFormType, int>> GetEmailSetupMappingAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NotifyStaffEmailSetupId.HasValue)
                .ToDictionaryAsync(k => k.VolunteerFormType, v => v.NotifyStaffEmailSetupId.Value);
        }

        public async Task<IDictionary<VolunteerFormType, int>> GetEmailSetupOverflowMappingAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NotfiyStaffOverflowEmailSetupId.HasValue)
                .ToDictionaryAsync(k => k.VolunteerFormType, 
                    v => v.NotfiyStaffOverflowEmailSetupId.Value);
        }

        public async Task<DataWithCount<ICollection<VolunteerForm>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<VolunteerForm>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.VolunteerFormType)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task UpdateSaveAsync(VolunteerForm form)
        {
            Update(form);
            await SaveAsync();
        }
    }
}