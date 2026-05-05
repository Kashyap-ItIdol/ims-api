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
            try
            {
                // Get template from database
                var template = await _templateRepository.GetByNameAsync("ForgotPassword");

                if (template == null)
                {
                    throw new InvalidOperationException("ForgotPassword email template not found in database.");
                }

                string subject = template.Subject;
                string htmlBody = template.BodyHtml;

                // Replace placeholders
                htmlBody = htmlBody.Replace("{otp}", otp.ToString())
                                  .Replace("{email}", toEmail);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_mailSettings.SenderName ?? "IMS", _mailSettings.SenderEmail ?? "no-reply@ims.com"));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

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
                        // Replace logo placeholder with embedded image
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

                _logger.LogInformation("OTP email sent successfully to {Email}", toEmail);
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
                return Result<bool>.Failure("Failed to send email.", 500);
            }
        }
    }
}
