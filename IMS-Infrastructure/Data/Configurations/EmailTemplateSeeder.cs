using IMS_Application.Interfaces;
using IMS_Domain.Entities;

namespace IMS_Infrastructure.Data.Configurations
{
    public static class EmailTemplateSeeder
    {
        private static readonly string ForgotPasswordHtml = @"<!DOCTYPE html>
<html>
<head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        @media only screen and (max-width: 480px) {
            .container { padding: 20px 10px !important; }
            .card { padding: 30px 20px !important; border-radius: 20px !important; }
            .otp-box { font-size: 32px !important; letter-spacing: 10px !important; padding: 15px !important; }
            .title { font-size: 28px !important; }
        }
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #f8fafc; font-family: Segoe UI, Arial, sans-serif;"">
    <div class=""container"" style=""background-color: #f8fafc; padding: 50px 0; text-align: center;"">
        
        <div class=""card"" style=""display: inline-block; background: radial-gradient(circle at center, #ffffff 0%, #eef6ff 100%); background-color: #ffffff; width: 90%; max-width: 500px; border-radius: 30px; padding: 50px 40px; border: 1px solid #e2e8f0; box-shadow: 0 10px 25px rgba(0,0,0,0.03); text-align: center;"">                            
            <div style=""margin-bottom: 25px;"">
                {logo}
            </div>
            <h1 class=""title"" style=""color: #2563eb; margin: 0 0 15px 0; font-size: 34px; font-weight: 700; letter-spacing: -0.5px;"">Password Reset Code</h1>                            
            <p style=""color: #64748b; font-size: 16px; margin: 0; line-height: 1.6;"">We received a request to reset your password for</p>
            <p style=""color: #1e293b; font-size: 16px; font-weight: 600; margin: 4px 0 12px 0;"">{email}</p>
            <p style=""color: #64748b; font-size: 15px; margin-bottom: 35px;"">Use the verification code below to continue.</p>
            <div class=""otp-box"" style=""background-color: #ffffff; border-radius: 12px; padding: 20px 40px; display: inline-block; box-shadow: 0 4px 12px rgba(0,0,0,0.05); border: 1px solid #f1f5f9; letter-spacing: 15px; font-size: 42px; font-weight: 700; color: #0f172a;"">
                <span style=""margin-left: 15px;"">{otp}</span>
            </div>
            <p style=""color: #94a3b8; font-size: 13px; margin-top: 50px; line-height: 1.6;"">
                This code will expire in 10 minutes.<br>
                If you did not request this, you can safely ignore this email.
            </p>
        </div>
    </div>
</body>
</html>";

        private static readonly string NewUserPasswordHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc;"">
    <!-- Main Background Container -->
    <div style=""padding: 60px 20px; min-height: 100vh; display: flex; align-items: center; justify-content: center;"">
        
        <!-- Card Container -->
        <div style="" max-width: 480px; width: 100%; margin: 0 auto; background: linear-gradient(180deg, #ffffff 0%, #deecfc 100%); border-radius: 40px; padding: 50px 40px; text-align: center; box-shadow: 0 20px 40px rgba(0,0,0,0.05);"">
            
            <!-- Logo Icon -->
            <div style=""margin-bottom: 30px;"">
                <div style=""margin-bottom: 25px;"">
                {logo}
            </div>
            </div>

            <!-- Main Heading -->
            <h1 style=""color: #0f172a; font-size: 28px; line-height: 1.2; margin-bottom: 16px; font-weight: 700; letter-spacing: -0.5px;"">
                Your <span style=""color: #2563eb;"">{roleTitle}</span><br>Account is Ready!
            </h1>

            <!-- Subtext -->
            <p style=""color: #64748b; font-size: 15px; line-height: 1.6; margin-bottom: 35px; max-width: 320px; margin-left: auto; margin-right: auto;"">
                You are now registered as a {roleTitle}, and you have full access to your dashboard.
            </p>

            <!-- Login Credentials Box -->
            <div style=""background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 16px; padding: 24px; text-align: left; margin-bottom: 35px;"">
                <p style=""margin: 0 0 12px 0; font-weight: 700; color: #1e293b; font-size: 15px;"">Your Login Credentials</p>
                
                <div style=""margin-bottom: 8px;"">
                    <span style=""font-size: 13px; color: #64748b;"">Username: </span>
                    <span style=""font-size: 13px; color: #1e293b; font-weight: 500;"">{email}</span>
                </div>
                
                <div>
                    <span style=""font-size: 13px; color: #64748b;"">Password: </span>
                    <span style=""font-size: 13px; color: #1e293b; font-weight: 500;"">{password}</span>
                </div>
            </div>

            <!-- CTA Button -->
            <a href=""{{loginUrl}}"" style=""display: inline-block; background-color: #2563eb; color: #ffffff; padding: 14px 32px; border-radius: 12px; text-decoration: none; font-weight: 600; font-size: 16px; transition: background-color 0.2s;"">
                Get started &nbsp; &rarr;
            </a>
        </div>
    </div>
</body>
</html>";
        public static async Task SeedAsync(IEmailTemplateRepository repository)
        {
            var existingTemplates = await repository.GetAllAsync();
            var templateList = existingTemplates.ToList();

            if (!templateList.Any(t => t.Name == "ForgotPassword"))
            {
                var forgotPasswordTemplate = new EmailTemplate
                {
                    Name = "ForgotPassword",
                    Subject = "Password Reset Code",
                    BodyHtml = ForgotPasswordHtml,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await repository.AddAsync(forgotPasswordTemplate);
            }

            if (!templateList.Any(t => t.Name == "NewUserPassword"))
            {
                var newUserPasswordTemplate = new EmailTemplate
                {
                    Name = "NewUserPassword",
                    Subject = "Your IMS account password",
                    BodyHtml = NewUserPasswordHtml,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await repository.AddAsync(newUserPasswordTemplate);
            }
        }
    }
}
