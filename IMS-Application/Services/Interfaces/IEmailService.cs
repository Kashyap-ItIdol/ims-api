using IMS_Application.Common.Models;

namespace IMS_Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task<Result<bool>> SendOtpAsync(string toEmail, int otp);
        Task<Result<bool>> SendNewUserPasswordAsync(string toEmail, string fullName, string password, string roleTitle);
    }
}
