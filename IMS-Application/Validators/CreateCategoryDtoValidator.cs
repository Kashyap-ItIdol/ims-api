using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryRequestDto>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MinimumLength(2).WithMessage("Category name must be at least 2 characters.")
                .Matches("^[a-zA-Z0-9 &-_]*$").WithMessage("Category name contains invalid characters.");
        }
    }

}
