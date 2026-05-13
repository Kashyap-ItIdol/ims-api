using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Interfaces;
using IMS_Application.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;

namespace IMS_Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailTemplateRepository _templateRepository;

        public EmailService(
            IOptions<MailSettings> mailSettings,
            ILogger<EmailService> logger,
            IEmailTemplateRepository templateRepository)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
            _templateRepository = templateRepository;
        }

        public async Task<Result<bool>> SendOtpAsync(string toEmail, int otp)
        {
            return await SendTemplateEmailAsync(
                toEmail: toEmail,
                templateName: "ForgotPassword",
                subject: null,
                replaceHtml: html => html.Replace("{otp}", otp.ToString())
                                          .Replace("{email}", toEmail),
                logContext: $"OTP to {toEmail}");
        }

        public async Task<Result<bool>> SendNewUserPasswordAsync(string toEmail, string fullName, string password, string roleTitle)
        {
            return await SendTemplateEmailAsync(
                toEmail: toEmail,
                templateName: "NewUserPassword",
                subject: null,
                replaceHtml: html => html.Replace("{email}", toEmail)
                                          .Replace("{fullName}", fullName)
                                          .Replace("{password}", password)
                                          .Replace("{roleTitle}", roleTitle),
                logContext: $"New user password to {toEmail}");
        }

        private async Task<Result<bool>> SendTemplateEmailAsync(string toEmail,string templateName, string? subject,Func<string, string> replaceHtml,string logContext)
        {
            try
            {
                var template = await _templateRepository.GetByNameAsync(templateName);
                if (template == null)
                    throw new InvalidOperationException($"{templateName} email template not found in database.");

                var subjectValue = subject ?? template.Subject;
                var htmlBody = replaceHtml(template.BodyHtml);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_mailSettings.SenderName ?? "IMS", _mailSettings.SenderEmail ?? "no-reply@ims.com"));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subjectValue;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };

                try
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "EmailIcon", "logo.png");
                    if (File.Exists(imagePath))
                    {
                        var image = bodyBuilder.LinkedResources.Add(imagePath);
                        image.ContentId = MimeUtils.GenerateMessageId();
                        htmlBody = htmlBody.Replace("{logo}", $"<img src=\"cid:{image.ContentId}\" width=\"75\" height=\"75\" alt=\"IMS Logo\" />");
                        bodyBuilder.HtmlBody = htmlBody;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not embed logo image, using default");
                    htmlBody = htmlBody.Replace("{logo}", "");
                    bodyBuilder.HtmlBody = htmlBody;
                }

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_mailSettings.SenderEmail, _mailSettings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully ({LogContext})", logContext);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email ({LogContext})", logContext);
                return Result<bool>.Failure("Failed to send email.", 500);
            }
        }
    }
}
