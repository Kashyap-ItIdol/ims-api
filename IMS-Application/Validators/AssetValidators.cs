using FluentValidation;
using IMS_Application.DTOs;
using IMS_Application.Common.Constants;

namespace IMS_Application.Validators
{
    public class CreateAssetDtoValidator : AbstractValidator<CreateAssetDto>
    {
        public CreateAssetDtoValidator()
        {
            RuleFor(x => x.AssetName)
                .NotEmpty().WithMessage("Asset name is required.")
                .MaximumLength(100).WithMessage("Asset name cannot be more than 100 characters.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => status == AssetStatus.Assigned || status == AssetStatus.Returned)
                .WithMessage("Status must be either 'Assigned' or 'Returned'.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID is required.")
                .GreaterThan(0).WithMessage("Category ID must be greater than 0.");

            RuleFor(x => x.SubCategoryId)
                .NotEmpty().WithMessage("Subcategory ID is required.")
                .GreaterThan(0).WithMessage("Subcategory ID must be greater than 0.");

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Brand is required.")
                .MaximumLength(50).WithMessage("Brand cannot exceed 50 characters.");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required")
                .MaximumLength(50).WithMessage("Model cannot exceed 50 characters");

            RuleFor(x => x.SerialNumber)
                .NotEmpty().WithMessage("Serial number is required.")
                .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters.");

            RuleFor(x => x.Condition)
                .NotEmpty().WithMessage("Condition is required.")
                .Must(condition => condition == AssetCondition.Good || condition == AssetCondition.Bad)
                .WithMessage("Condition must be either 'Good' or 'Bad'.");

            RuleFor(x => x.ClientPOC)
                .NotEmpty().WithMessage("ClientPOC is required.")
                .MaximumLength(100).WithMessage("ClientPOC cannot exceed 100 characters.");

            RuleFor(x => x.SalesPOC)
                .NotEmpty().WithMessage("SalesPOC is required.")
                .MaximumLength(100).WithMessage("SalesPOC cannot exceed 100 characters.");

            RuleFor(x => x.CreatedBy)
                .NotEmpty().WithMessage("CreatedBy is required.")
                .GreaterThan(0).WithMessage("CreatedBy must be valid");
        }
    }
}
