using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Email;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class ScheduleNotificationService
        : BaseService<ScheduleNotificationService>, IScheduleNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IScheduleRequestService _scheduleRequestService;
        private readonly ISiteSettingService _siteSettingService;

        public ScheduleNotificationService(ILogger<ScheduleNotificationService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IScheduleRequestService scheduleRequestService,
            ISiteSettingService siteSettingService) : base(logger, httpContextAccessor)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _scheduleRequestService = scheduleRequestService
                ?? throw new ArgumentNullException(nameof(scheduleRequestService));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public async Task<Configuration> GetEmailSettingsAsync()
        {
            var config = new Configuration
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

        public async Task SendPendingNotificationsAsync()
        {
            var pendingNotifications
                = await _scheduleRequestService.GetPendingNotificationsAsync();

            if (pendingNotifications?.Count > 0)
            {
                _logger.LogDebug("Found {PendingNotificationCount} pending notifications",
                    pendingNotifications.Count);

                Configuration settings = null;
                try
                {
                    settings = await GetEmailSettingsAsync();
                }
                catch (OcudaEmailException) { }

                if (settings != null)
                {
                    foreach (var pending in pendingNotifications)
                    {
                        var setupId = (int)pending.ScheduleRequestSubject.RelatedEmailSetupId;
                        var lang = pending.Language
                            .Equals("English", StringComparison.OrdinalIgnoreCase)
                                ? "en-US"
                                : pending.Language;

                        var emailSetupText = await _emailService.GetEmailSetupAsync(setupId, lang);

                        var emailTemplateText = await _emailService
                            .GetEmailTemplateAsync(emailSetupText.EmailSetup.EmailTemplateId, lang);

                        var emailDetails = new Details
                        {
                            FromEmailAddress = emailSetupText.EmailSetup.FromEmailAddress,
                            FromName = emailSetupText.EmailSetup.FromName,
                            UrlParameters = emailSetupText.UrlParameters,
                            Preview = emailSetupText.Preview,
                            TemplateHtml = emailTemplateText.TemplateHtml,
                            TemplateText = emailTemplateText.TemplateText,
                            BodyText = emailSetupText.BodyText,
                            BodyHtml = emailSetupText.BodyHtml,
                            Tags = new Dictionary<string, string>
                            {
                                { "Scheduled", pending.RequestedTime.ToString() },
                                { "Subject", pending.ScheduleRequestSubject.Subject }
                            },
                            BccEmailAddress = settings.BccAddress,
                            OverrideEmailToAddress = settings.OverrideToAddress,
                            Password = settings.OutgoingPassword,
                            Port = settings.OutgoingPort,
                            RestrictToDomain = settings.RestrictToDomain,
                            Server = settings.OutgoingHost,
                            Subject = emailSetupText.Subject,
                            Username = settings.OutgoingLogin
                        };

                        try
                        {
                            await _emailService.SendAsync(emailDetails,
                                pending.Email.Trim(),
                                pending.Name?.Trim());
                            await _scheduleRequestService.SetNotificationSentAsync(pending.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                                emailSetupText.EmailSetup.Id,
                                pending.Email.Trim(),
                                ex.Message);
                        }
                    }
                }
            }
        }
    }
}
