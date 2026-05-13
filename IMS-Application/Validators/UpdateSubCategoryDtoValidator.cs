using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators;

public class UpdateSubCategoryDtoValidator : AbstractValidator<UpdateSubCategoryDto>
{
    public UpdateSubCategoryDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");
    }
}
