using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class VolunteerFormSubmissionEmailRecordRepository
        : GenericRepository<OpsContext, VolunteerFormSubmissionEmailRecord>,
        IVolunteerFormSubmissionEmailRecordRepository
    {
        public VolunteerFormSubmissionEmailRecordRepository(Repository<OpsContext> repositoryFacade,
            ILogger<VolunteerFormSubmissionEmailRecordRepository> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(int formSubmissionId, int emailRecordId, int userId)
        {
            await DbSet.AddAsync(new VolunteerFormSubmissionEmailRecord
            {
                VolunterFormSubmissionId = formSubmissionId,
                EmailRecordId = emailRecordId,
                UserId = userId
            });
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<VolunteerFormSubmissionEmailRecord>>
            GetBySubmissionId(int submissionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.VolunterFormSubmissionId == submissionId)
                .Include(_ => _.EmailRecord)
                .Include(_ => _.User)
                .ToListAsync();
        }
    }
}