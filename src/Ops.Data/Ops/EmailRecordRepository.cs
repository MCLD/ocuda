using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class EmailRecordRepository
        : GenericRepository<OpsContext, EmailRecord>, IEmailRecordRepository
    {
        public EmailRecordRepository(Repository<OpsContext> repositoryFacade,
            ILogger<EmailRecordRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public Task<EmailRecord> AddSaveAsync(EmailRecord emailRecord)
        {
            if (emailRecord == null)
            {
                throw new ArgumentNullException(nameof(emailRecord));
            }
            return AddSaveInternalAsync(emailRecord);
        }

        private async Task<EmailRecord> AddSaveInternalAsync(EmailRecord emailRecord)
        {
            emailRecord.CreatedAt = DateTime.Now;
            var result = await DbSet.AddAsync(emailRecord);
            await _context.SaveChangesAsync();
            return result.Entity;
        }
    }
}