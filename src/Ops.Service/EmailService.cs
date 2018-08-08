using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly ISiteSettingService _siteSettingService;
        private readonly IUserService _userService;
        public EmailService(ILogger<EmailService> logger,
            ISiteSettingService siteSettingService,
            IUserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task SendToUserAsync(int userId, string subject, string body,
            string htmlBody = null)
        {
            var user = await _userService.GetByIdAsync(userId);

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new OcudaException("User does not have an email address configured.");
            }

            await SendEmailAsync(user.Email,
                subject,
                body,
                htmlBody,
                user.Nickname);
        }

        public async Task SendToAddressAsync(string emailAddress, string subject,
            string body, string htmlBody = null)
        {
            await SendEmailAsync(emailAddress,
                subject,
                body,
                htmlBody);
        }

        private async Task SendEmailAsync(string emailAddress,
            string subject,
            string body,
            string htmlBody = null,
            string emailName = null)
        {
            var message = new MimeMessage();

            var fromName = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.Email.FromName);
            var fromAddress = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.Email.FromAddress);
            message.From.Add(new MailboxAddress(fromName, fromAddress));

            if (!string.IsNullOrWhiteSpace(emailName))
            {
                message.To.Add(new MailboxAddress(emailName, emailAddress));
            }
            else
            {
                message.To.Add(new MailboxAddress(emailAddress));
            }

            message.Subject = subject;

            var builder = new BodyBuilder();
            builder.TextBody = body;
            if (!string.IsNullOrWhiteSpace(htmlBody))
            {
                builder.HtmlBody = htmlBody;
            }
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                // accept any STARTTLS certificate
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                var outgoingHost = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.Email.OutgoingHost);
                var outgoingPort = await _siteSettingService.GetSettingIntAsync(
                    Ops.Models.Keys.SiteSetting.Email.OutgoingPort);

                await client.ConnectAsync(outgoingHost, outgoingPort, false);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                var outgoingLogin = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.Email.OutgoingLogin);
                var outgoingPassword = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.Email.OutgoingPassword);
                if (!string.IsNullOrEmpty(outgoingLogin)
                    && !string.IsNullOrEmpty(outgoingPassword))
                {
                    client.Authenticate(outgoingLogin, outgoingPassword);
                }

                try
                {
                    await client.SendAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unable to send email: {ex.Message}");
                    throw new OcudaException("Unable to send email.", ex);
                }
                await client.DisconnectAsync(true);
            }
        }
    }
}
