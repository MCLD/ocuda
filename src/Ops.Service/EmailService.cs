using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CommonMark;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class EmailService : BaseService<EmailService>, IEmailService
    {
        private const int CacheEmailHours = 1;

        private readonly IOcudaCache _cache;
        private readonly IEmailRecordRepository _emailRecordRepository;
        private readonly IEmailSetupTextRepository _emailSetupTextRepository;
        private readonly IEmailTemplateTextRepository _emailTemplateTextRepository;
        private readonly Utility.Email.Sender _sender;
        private readonly ISiteSettingService _siteSettingService;

        public EmailService(ILogger<EmailService> logger,
            IEmailRecordRepository emailRecordRepository,
            IEmailSetupTextRepository emailSetupTextRepository,
            IEmailTemplateTextRepository emailTemplateTextRepository,
            IHttpContextAccessor httpContextAccessor,
            IOcudaCache cache,
            ISiteSettingService siteSettingService,
            Utility.Email.Sender sender)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(emailRecordRepository);
            ArgumentNullException.ThrowIfNull(emailSetupTextRepository);
            ArgumentNullException.ThrowIfNull(emailTemplateTextRepository);
            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(siteSettingService);

            _cache = cache;
            _emailRecordRepository = emailRecordRepository;
            _emailSetupTextRepository = emailSetupTextRepository;
            _emailTemplateTextRepository = emailTemplateTextRepository;
            _sender = sender;
            _siteSettingService = siteSettingService;
        }

        public async Task<Utility.Email.Details> GetDetailsAsync(int emailSetupId,
             string languageName,
             IDictionary<string, string> tags)
        {
            var emailSetupText = await GetEmailSetupAsync(emailSetupId, languageName)
                ?? throw new OcudaEmailException($"Unable to find email setup {emailSetupId} in the requested or default language.");

            var emailTemplateText
                = await GetEmailTemplateAsync(emailSetupText.EmailSetup.EmailTemplateId,
                    languageName)
                ?? throw new OcudaEmailException($"Unable to find email template {emailSetupText.EmailSetup.EmailTemplateId} in the requested or default language.");

            var settings = await GetEmailSettingsAsync();

            if (string.IsNullOrEmpty(emailSetupText.BodyHtml))
            {
                try
                {
                    emailSetupText.BodyHtml = CommonMarkConverter.Convert(emailSetupText.BodyText);
                }
                catch (CommonMarkException cmex)
                {
                    _logger.LogError(cmex,
                        "Error converting text format email to HTML with CommonMark: {ErrorMessage}",
                        cmex.Message);
                }
            }

            return new Utility.Email.Details(tags)
            {
                BccEmailAddress = settings.BccAddress,
                BodyHtml = emailSetupText.BodyHtml,
                BodyText = emailSetupText.BodyText,
                FromEmailAddress = emailSetupText.EmailSetup.FromEmailAddress,
                FromName = emailSetupText.EmailSetup.FromName,
                OverrideEmailToAddress = settings.OverrideToAddress,
                Password = settings.OutgoingPassword,
                Port = settings.OutgoingPort,
                Preview = emailSetupText.Preview,
                RestrictToDomain = settings.RestrictToDomain,
                Server = settings.OutgoingHost,
                Subject = emailSetupText.Subject,
                TemplateHtml = emailTemplateText.TemplateHtml,
                TemplateText = emailTemplateText.TemplateText,
                UrlParameters = emailSetupText.UrlParameters,
                Username = settings.OutgoingLogin
            };
        }

        public async Task<EmailRecord> SendAsync(Utility.Email.Details emailDetails)
        {
            var record = await _sender.SendEmailAsync(emailDetails);

            if (record != null)
            {
                try
                {
                    var emailRecord = new EmailRecord(record)
                    {
                        CreatedAt = DateTime.Now
                    };
                    return await _emailRecordRepository.AddSaveAsync(emailRecord);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unable to save email record for email sent to {ToAddress}: {ErrorMessage}",
                        emailDetails?.ToEmailAddress ?? "unknown",
                        ex.Message);
                }
            }

            return null;
        }

        private async Task<Utility.Email.Configuration> GetEmailSettingsAsync()
        {
            var config = new Utility.Email.Configuration
            {
                BccAddress = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.BccAddress),
                FromAddress = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.FromAddress),
                FromName = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.FromName),
                OutgoingHost = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.OutgoingHost),
                OutgoingLogin = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.OutgoingLogin),
                OutgoingPassword = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.OutgoingPassword),
                OutgoingPort = await _siteSettingService
                    .GetSettingIntAsync(Ops.Models.Keys.SiteSetting.Email.OutgoingPort),
                OverrideToAddress = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.OverrideToAddress),
                RestrictToDomain = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.RestrictToDomain)
            };

            if (string.IsNullOrEmpty(config.FromAddress))
            {
                throw new OcudaEmailException("From address is not configured");
            }

            if (string.IsNullOrEmpty(config.FromName))
            {
                throw new OcudaEmailException("From name is not configured");
            }

            if (string.IsNullOrEmpty(config.OutgoingHost))
            {
                throw new OcudaEmailException("Outgoing mail host is not configured");
            }

            return config;
        }

        private async Task<EmailSetupText>
            GetEmailSetupAsync(int emailSetupId, string languageName)
        {
            var emailSetup = await InternalGetEmailSetupAsync(emailSetupId, languageName);
            return emailSetup
                ?? await InternalGetEmailSetupAsync(emailSetupId, i18n.Culture.DefaultName);
        }

        private async Task<EmailTemplateText> GetEmailTemplateAsync(int emailTemplateId,
            string languageName)
        {
            var emailTemplate = await InternalGetEmailTemplateAsync(emailTemplateId, languageName);
            return emailTemplate
                ?? await InternalGetEmailTemplateAsync(emailTemplateId, i18n.Culture.DefaultName);
        }

        private async Task<EmailSetupText> InternalGetEmailSetupAsync(int emailSetupId,
                                    string languageName)
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Cache.OpsEmailSetup,
                emailSetupId,
                languageName);

            var emailSetup = await _cache
                .GetObjectFromCacheAsync<EmailSetupText>(cacheKey);

            if (emailSetup == null)
            {
                emailSetup = await _emailSetupTextRepository
                    .GetByIdLanguageAsync(emailSetupId, languageName);

                if (emailSetup != null)
                {
                    await _cache.SaveToCacheAsync(cacheKey, emailSetup, CacheEmailHours);
                }
            }

            return emailSetup;
        }

        private async Task<EmailTemplateText> InternalGetEmailTemplateAsync(int emailTemplateId,
            string languageName)
        {
            var cacheKey = string.Format(
                CultureInfo.InvariantCulture,
                Cache.OpsEmailTemplate,
                emailTemplateId,
                languageName);

            var emailTemplate = await _cache.GetObjectFromCacheAsync<EmailTemplateText>(cacheKey);

            if (emailTemplate == null)
            {
                emailTemplate = await _emailTemplateTextRepository
                    .GetByIdLanguageAsync(emailTemplateId, languageName);

                if (emailTemplate != null)
                {
                    await _cache.SaveToCacheAsync(cacheKey, emailTemplate, CacheEmailHours);
                }
            }

            return emailTemplate;
        }
    }
}