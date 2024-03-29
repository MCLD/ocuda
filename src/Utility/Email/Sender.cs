﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ocuda.Utility.Exceptions;
using Serilog.Context;
using Stubble.Core.Builders;

namespace Ocuda.Utility.Email
{
    public class Sender
    {
        private readonly ILogger _logger;

        public Sender(ILogger<Sender> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Record> SendEmailAsync(Details details)
        {
            if (details == null)
            {
                throw new ArgumentNullException(nameof(details));
            }

            if (string.IsNullOrWhiteSpace(details.ToEmailAddress))
            {
                throw new OcudaEmailException("No to email address provided.");
            }

            if (string.IsNullOrWhiteSpace(details.Server))
            {
                throw new OcudaEmailException("No server provided.");
            }

            if (string.IsNullOrWhiteSpace(details.FromEmailAddress))
            {
                throw new OcudaEmailException("No email from address provided.");
            }

            if (string.IsNullOrWhiteSpace(details.Subject))
            {
                throw new OcudaEmailException("No email subject provided.");
            }

            if (string.IsNullOrWhiteSpace(details.BodyHtml))
            {
                throw new OcudaEmailException("No body HTML provided.");
            }

            if (string.IsNullOrWhiteSpace(details.BodyText))
            {
                throw new OcudaEmailException("No body text provided.");
            }

            if (string.IsNullOrEmpty(details.OverrideEmailToAddress)
                && !string.IsNullOrEmpty(details.RestrictToDomain)
                && !details.ToEmailAddress.EndsWith(details.RestrictToDomain,
                    StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Requested to send email to {ToEmailAddress} which doesn't align with the restricted to domain {RestrictToDomain}",
                    details.ToEmailAddress,
                    details.RestrictToDomain);
                throw new OcudaEmailException($"Restricted to sending emails to the following domain: {details.RestrictToDomain}");
            }

            return SendEmailInternalAsync(details);
        }

        private async Task<Record> SendEmailInternalAsync(Details details)
        {
            // apply the details replacements
            _logger.LogTrace("Applying details replacements for {TagCount} tags",
                details.Tags?.Count ?? 0);

            if (details.Tags?.Count > 0)
            {
                var stubble = new StubbleBuilder().Build();
                details.BodyHtml = await stubble.RenderAsync(details.BodyHtml, details.Tags);
                details.BodyText = await stubble.RenderAsync(details.BodyText, details.Tags);
                details.Preview = await stubble.RenderAsync(details.Preview, details.Tags);
                details.Subject = await stubble.RenderAsync(details.Subject, details.Tags);
            }

            // apply the sections to the HTML template
            if (!string.IsNullOrEmpty(details?.TemplateHtml))
            {
                _logger.LogTrace("Performing HTML template replacement");
                details.BodyHtml = details.TemplateHtml
                    .Replace("{{Body}}", details.BodyHtml, StringComparison.OrdinalIgnoreCase)
                    .Replace("{{Preview}}", details.Preview, StringComparison.OrdinalIgnoreCase)
                    .Replace("{{UrlParameters}}", details.UrlParameters,
                        StringComparison.OrdinalIgnoreCase);
            }

            // apply the text template
            if (details?
                    .TemplateText?
                    .Contains("{{Body}}", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogTrace("Performing text template replacement");
                details.BodyText = details.TemplateText.Replace("{{Body}}",
                    details.BodyText,
                    StringComparison.OrdinalIgnoreCase);
            }

            using var message = new MimeMessage
            {
                Subject = details.Subject,
                Body = new BodyBuilder
                {
                    TextBody = details.BodyText,
                    HtmlBody = details.BodyHtml
                }.ToMessageBody()
            };

            message.From.Add(new MailboxAddress(details.FromName, details.FromEmailAddress));

            if (!string.IsNullOrWhiteSpace(details.OverrideEmailToAddress))
            {
                message.To.Add(MailboxAddress.Parse(details.OverrideEmailToAddress));
            }
            else
            {
                message.To.Add(string.IsNullOrWhiteSpace(details.ToName)
                    ? MailboxAddress.Parse(details.ToEmailAddress)
                    : new MailboxAddress(details.ToName, details.ToEmailAddress));
            }

            if (!string.IsNullOrWhiteSpace(details.BccEmailAddress))
            {
                message.Bcc.Add(MailboxAddress.Parse(details.BccEmailAddress));
            }

            if (details.Cc?.Count > 0)
            {
                foreach (var item in details.Cc)
                {
                    if (!string.IsNullOrWhiteSpace(details.OverrideEmailToAddress))
                    {
                        message.Cc.Add(MailboxAddress.Parse(details.OverrideEmailToAddress));
                    }
                    else
                    {
                        if (item.Key == item.Value)
                        {
                            message.Cc.Add(MailboxAddress.Parse(item.Key));
                        }
                        else
                        {
                            message.Cc.Add(new MailboxAddress(item.Value, item.Key));
                        }
                    }
                }
            }

            using var client = new SmtpClient
            {
                // accept any STARTTLS certificate
                ServerCertificateValidationCallback = (_, __, ___, ____) => true,
            };

            client.MessageSent += (sender, e) =>
            {
                details.SentResponse = e.Response?.Length > 255
                    ? e.Response.Substring(0, 255)
                    : e.Response;
            };

            client.AuthenticationMechanisms.Remove("XOAUTH2");

            client.Timeout = 30 * 1000;  // 30 seconds

            var sendTimer = Stopwatch.StartNew();

            _logger.LogTrace("Connecting to server {MailServer} on port {MailServerPort}",
                details.Server,
                details.Port ?? 25);

            await client.ConnectAsync(details.Server,
                details.Port ?? 25,
                MailKit.Security.SecureSocketOptions.None);

            if (!string.IsNullOrWhiteSpace(details.Username)
                && !string.IsNullOrWhiteSpace(details.Password))
            {
                await client.AuthenticateAsync(details.Username, details.Password);
            }

            try
            {
                _logger.LogTrace("Calling SMTP client send at {TimeStamp} ms",
                    sendTimer.ElapsedMilliseconds);

                await client.SendAsync(message);

                _logger.LogTrace("SMTP send complete at {TimeStamp} ms",
                    sendTimer.ElapsedMilliseconds);

                sendTimer.Stop();

                using (LogContext.PushProperty("EmailServer", details.Server))
                using (LogContext.PushProperty("EmailPort", details.Port))
                using (LogContext.PushProperty("EmailRestrictToDomain", details.RestrictToDomain))
                using (LogContext.PushProperty("EmailUsername", details.Username))
                using (LogContext.PushProperty("EmailBccToAddress", details.BccEmailAddress))
                using (LogContext.PushProperty("EmailSubject", details.Subject))
                using (LogContext.PushProperty("EmailFromName", details.FromName))
                using (LogContext.PushProperty("EmailFromAddress", details.FromEmailAddress))
                using (LogContext.PushProperty("EmailToAddressOverride", details.OverrideEmailToAddress))
                using (LogContext.PushProperty("EmailServerResponse", details.SentResponse))
                {
                    _logger.LogInformation("Email sent to {EmailAddress} subject {EmailSubject} in {Elapsed} ms",
                        string.IsNullOrWhiteSpace(details.OverrideEmailToAddress)
                            ? details.ToEmailAddress
                            : details.OverrideEmailToAddress,
                        details.Subject,
                        sendTimer.ElapsedMilliseconds);
                }

                return details;
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                _logger.LogError("Error sending email to: {EmailAddress} - status {StatusCode}, {ErrorMessage}",
                    string.IsNullOrWhiteSpace(details.OverrideEmailToAddress)
                            ? details.ToEmailAddress
                            : details.OverrideEmailToAddress,
                    ex.StatusCode,
                    ex.Message);
                return null;
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
