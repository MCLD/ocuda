using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Email;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class ScheduleNotificationService
        : BaseService<ScheduleNotificationService>, IScheduleNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IScheduleLogRepository _scheduleLogRepository;
        private readonly IScheduleRequestService _scheduleRequestService;
        private readonly ISiteSettingService _siteSettingService;

        public ScheduleNotificationService(ILogger<ScheduleNotificationService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IScheduleLogRepository scheduleLogRepsitory,
            IScheduleRequestService scheduleRequestService,
            ISiteSettingService siteSettingService) : base(logger, httpContextAccessor)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _scheduleLogRepository = scheduleLogRepsitory
                ?? throw new ArgumentNullException(nameof(scheduleLogRepsitory));
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

        public async Task<int> SendPendingNotificationsAsync()
        {
            int sentNotifications = 0;
            var pendingNotifications
                = await _scheduleRequestService.GetPendingNotificationsAsync();

            if (pendingNotifications?.Count > 0)
            {
                _logger.LogDebug("Found {PendingNotificationCount} pending notification(s)",
                    pendingNotifications.Count);

                Configuration settings = null;
                try
                {
                    settings = await GetEmailSettingsAsync();
                }
                catch (OcudaEmailException oex)
                {
                    _logger.LogError("Error finding email settings: {ErrorMessage}", oex.Message);
                }

                if (settings != null)
                {
                    foreach (var pending in pendingNotifications)
                    {
                        try
                        {
                            var setupId = (int)pending.ScheduleRequestSubject.RelatedEmailSetupId;
                            _logger.LogTrace("Email setup id: {EmailSetupId}", setupId);

                            var lang = pending.Language
                                .Equals("English", StringComparison.OrdinalIgnoreCase)
                                    ? "en-US"
                                    : pending.Language;
                            _logger.LogTrace("Using language: {Language}", pending.Language);

                            var culture = CultureInfo.GetCultureInfo(lang);
                            _logger.LogTrace("Found culture: {Culture}", culture.DisplayName);

                            var emailSetupText = await _emailService.GetEmailSetupAsync(setupId, lang);
                            _logger.LogTrace("Email setup fetched, subject: {Subject}", emailSetupText.Subject);


                            var emailTemplateText = await _emailService
                                .GetEmailTemplateAsync(emailSetupText.EmailSetup.EmailTemplateId, lang);
                            _logger.LogTrace("Email template for {Language}: HTML {HtmlLength} chars, text {TextLength} chars",
                                emailTemplateText.PromenadeLanguageName,
                                emailTemplateText.TemplateHtml.Length,
                                emailTemplateText.TemplateText.Length);

                            var emailDetails = new Details
                            {
                                FromEmailAddress = emailSetupText.EmailSetup.FromEmailAddress,
                                FromName = emailSetupText.EmailSetup.FromName,
                                ToEmailAddress = pending.Email.Trim(),
                                ToName = pending.Name?.Trim(),
                                UrlParameters = emailSetupText.UrlParameters,
                                Preview = emailSetupText.Preview,
                                TemplateHtml = emailTemplateText.TemplateHtml,
                                TemplateText = emailTemplateText.TemplateText,
                                BodyText = emailSetupText.BodyText,
                                BodyHtml = emailSetupText.BodyHtml,
                                Tags = new Dictionary<string, string>
                                {
                                    { "ScheduledDate", pending.RequestedTime.ToString("d", culture) },
                                    { "ScheduledTime", pending.RequestedTime.ToString("t", culture) },
                                    { "Scheduled", pending.RequestedTime.ToString("g", culture) },
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

                            _logger.LogTrace("Calling email service send async");

                            var sentEmail = await _emailService.SendAsync(emailDetails);

                            _logger.LogTrace("Back from email service send async");

                            if (sentEmail != null)
                            {
                                sentNotifications++;

                                await _scheduleRequestService.SetNotificationSentAsync(pending);

                                await _scheduleLogRepository.AddAsync(new ScheduleLog
                                {
                                    CreatedAt = DateTime.Now,
                                    Notes = $"Request confirmation email sent to {pending.Email.Trim()}.",
                                    ScheduleRequestId = pending.Id,
                                    RelatedEmailId = sentEmail?.Id
                                });
                                await _scheduleLogRepository.SaveAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Sending pending notification id {RequestId} failed: {ErrorMessage}",
                                pending.Id,
                                ex.Message);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
            }

            return sentNotifications;
        }

        public async Task SendFollowupAsync(ScheduleRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Configuration settings = null;
            try
            {
                settings = await GetEmailSettingsAsync();
            }
            catch (OcudaEmailException) { }

            if (settings != null)
            {
                var setupId = (int)request.ScheduleRequestSubject.FollowupEmailSetupId;
                var lang = request.Language
                    .Equals("English", StringComparison.OrdinalIgnoreCase)
                        ? "en-US"
                        : request.Language;

                var culture = CultureInfo.GetCultureInfo(lang);

                var emailSetupText = await _emailService.GetEmailSetupAsync(setupId, lang);

                var emailTemplateText = await _emailService
                    .GetEmailTemplateAsync(emailSetupText.EmailSetup.EmailTemplateId, lang);

                var emailDetails = new Details
                {
                    FromEmailAddress = emailSetupText.EmailSetup.FromEmailAddress,
                    FromName = emailSetupText.EmailSetup.FromName,
                    ToEmailAddress = request.Email.Trim(),
                    ToName = request.Name?.Trim(),
                    UrlParameters = emailSetupText.UrlParameters,
                    Preview = emailSetupText.Preview,
                    TemplateHtml = emailTemplateText.TemplateHtml,
                    TemplateText = emailTemplateText.TemplateText,
                    BodyText = emailSetupText.BodyText,
                    BodyHtml = emailSetupText.BodyHtml,
                    Tags = new Dictionary<string, string>
                            {
                                { "Scheduled", request.RequestedTime.ToString(culture) },
                                { "Subject", request.ScheduleRequestSubject.Subject }
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
                    var sentEmail = await _emailService.SendAsync(emailDetails);
                    if (sentEmail != null)
                    {
                        await _scheduleRequestService.SetFollowupSentAsync(request);

                        await _scheduleLogRepository.AddAsync(new ScheduleLog
                        {
                            CreatedAt = DateTime.Now,
                            Notes = $"Follow-up email sent to {request.Email.Trim()}.",
                            ScheduleRequestId = request.Id,
                            RelatedEmailId = sentEmail?.Id
                        });
                        await _scheduleLogRepository.SaveAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                        emailSetupText.EmailSetup.Id,
                        request.Email.Trim(),
                        ex.Message);
                }
            }
        }
    }
}
