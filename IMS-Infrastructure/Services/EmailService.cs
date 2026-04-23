using IMS_Application.Common.Models;
using IMS_Application.DTOs;
using IMS_Application.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils; // Required for MimeUtils.GenerateMessageId

namespace IMS_Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }
        public async Task<Result<bool>> SendOtpAsync(string toEmail, int otp)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_mailSettings.SenderName ?? "IMS", _mailSettings.SenderEmail ?? "no-reply@ims.com"));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = $"{otp} is your verification code";
                var bodyBuilder = new BodyBuilder();
                // 1. EMBED THE IMAGE
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "EmailIcon", "logo.png");
                var image = bodyBuilder.LinkedResources.Add(imagePath);
                image.ContentId = MimeUtils.GenerateMessageId();
                // 2. CONSTRUCT RESPONSIVE HTML
                bodyBuilder.HtmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                    <style>
                        @media only screen and (max-width: 480px) {{
                            .container {{ padding: 20px 10px !important; }}
                            .card {{ padding: 30px 20px !important; border-radius: 20px !important; }}
                            .otp-box {{ font-size: 32px !important; letter-spacing: 10px !important; padding: 15px !important; }}
                            .title {{ font-size: 28px !important; }}
                        }}
                    </style>
                </head>
                <body style=""margin: 0; padding: 0; background-color: #f8fafc; font-family: 'Segoe UI', Arial, sans-serif;"">
                    <div class=""container"" style=""background-color: #f8fafc; padding: 50px 0; text-align: center;"">
                        
                        <div class=""card"" style=""display: inline-block; background: radial-gradient(circle at center, #ffffff 0%, #eef6ff 100%); background-color: #ffffff; width: 90%; max-width: 500px; border-radius: 30px; padding: 50px 40px; border: 1px solid #e2e8f0; box-shadow: 0 10px 25px rgba(0,0,0,0.03); text-align: center;"">                            
                            <div style=""margin-bottom: 25px;"">
                                <img src=""cid:{image.ContentId}"" width=""75"" height=""75"" alt=""IMS Logo"" style=""display: inline-block; width: 75px; height: 75px;"" />
                            </div>
                            <h1 class=""title"" style=""color: #2563eb; margin: 0 0 15px 0; font-size: 34px; font-weight: 700; letter-spacing: -0.5px;"">Password Reset Code</h1>                            
                            <p style=""color: #64748b; font-size: 16px; margin: 0; line-height: 1.6;"">We received a request to reset your password for</p>
                            <p style=""color: #1e293b; font-size: 16px; font-weight: 600; margin: 4px 0 12px 0;"">{toEmail}</p>
                            <p style=""color: #64748b; font-size: 15px; margin-bottom: 35px;"">Use the verification code below to continue.</p>
                            <div class=""otp-box"" style=""background-color: #ffffff; border-radius: 12px; padding: 20px 40px; display: inline-block; box-shadow: 0 4px 12px rgba(0,0,0,0.05); border: 1px solid #f1f5f9; letter-spacing: 15px; font-size: 42px; font-weight: 700; color: #0f172a;"">
                                <span style=""margin-left: 15px;"">{otp}</span>
                            </div>
                            <p style=""color: #94a3b8; font-size: 13px; margin-top: 50px; line-height: 1.6;"">
                                This code will expire in 10 minutes.<br>
                                If you didn't request this, you can safely ignore this email.
                            </p>
                        </div>
                    </div>
                </body>
                </html>";
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