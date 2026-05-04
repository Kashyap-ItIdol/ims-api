using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators;

public class UpdateSubCategoryDtoValidator : AbstractValidator<UpdateSubCategoryDto>
{
    public UpdateSubCategoryDtoValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required")
            .GreaterThan(0).WithMessage("Category ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 100).WithMessage("Name must be between 2 and 100 characters");
    }
}
