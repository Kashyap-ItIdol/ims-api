using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MinimumLength(2).WithMessage("Full name must be at least 2 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords don't match");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Department is required");
        }
    }
}
