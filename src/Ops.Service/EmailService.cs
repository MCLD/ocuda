using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class EmailService : BaseService<EmailService>, IEmailService
    {
        private readonly Utility.Email.Sender _sender;
        private readonly IEmailRecordRepository _emailRecordRepository;
        private readonly IEmailSetupTextRepository _emailSetupTextRepository;
        private readonly IEmailTemplateTextRepository _emailTemplateTextRepository;

        public EmailService(ILogger<EmailService> logger,
            Utility.Email.Sender sender,
            IHttpContextAccessor httpContextAccessor,
            IEmailRecordRepository emailRecordRepository,
            IEmailSetupTextRepository emailSetupTextRepository,
            IEmailTemplateTextRepository emailTemplateTextRepository)
            : base(logger, httpContextAccessor)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _emailRecordRepository = emailRecordRepository
                ?? throw new ArgumentNullException(nameof(emailRecordRepository));
            _emailSetupTextRepository = emailSetupTextRepository
                ?? throw new ArgumentNullException(nameof(emailSetupTextRepository));
            _emailTemplateTextRepository = emailTemplateTextRepository
                ?? throw new ArgumentNullException(nameof(emailTemplateTextRepository));
        }

        public async Task<EmailSetupText> GetEmailSetupAsync(int emailSetupId, string languageName)
        {
            return await _emailSetupTextRepository
                .GetByIdLanguageAsync(emailSetupId, languageName);
        }

        public async Task<EmailTemplateText> GetEmailTemplateAsync(int emailTemplateId,
            string languageName)
        {
            return await _emailTemplateTextRepository
                .GetByIdLanguageAsync(emailTemplateId, languageName);
        }

        public async Task<Utility.Email.Record> SendAsync(Utility.Email.Details emailDetails)
        {
            var record = await _sender.SendEmailAsync(emailDetails);

            try
            {
                var emailRecord = new EmailRecord(record)
                {
                    CreatedAt = DateTime.Now
                };
                await _emailRecordRepository.AddSaveAsync(emailRecord);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to save email record for email sent to {ToAddress}: {ErrorMessage}",
                    emailDetails.ToEmailAddress,
                    ex.Message);
            }

            return record;
        }
    }
}
