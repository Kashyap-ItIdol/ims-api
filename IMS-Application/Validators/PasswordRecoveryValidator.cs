using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators.PasswordRecovery
{
    public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");
        }
    }

    public class OtpVerificationRequestValidator : AbstractValidator<OtpVerificationRequestDto>
    {
        public OtpVerificationRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Otp)
                .InclusiveBetween(1000, 9999).WithMessage("OTP must be 4 digits between 1000 and 9999");
        }
    }

    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.ResetToken)
                .NotEmpty().WithMessage("Reset token is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords don't match. Please double-check and try again");
        }
    }
}
