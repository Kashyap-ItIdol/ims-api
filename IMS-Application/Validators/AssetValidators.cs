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
                .MaximumLength(100).WithMessage("Asset name cannot be more than 100 characters.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => status == AssetStatus.Assigned || status == AssetStatus.Returned)
                .WithMessage("Status must be either 'Assigned' or 'Returned'.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be a greater than 1.");

            RuleFor(x => x.SubCategoryId)
                .GreaterThan(0).WithMessage("Subcategory ID must be a greater than 1.");

            RuleFor(x => x.Brand)
                .MaximumLength(50).WithMessage("Brand cannot exceed 50 characters.");

            RuleFor(x => x.Model)
                .MaximumLength(50).WithMessage("Model cannot exceed 50 characters");

            RuleFor(x => x.SerialNumber)
                .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters.");

            RuleFor(x => x.ConditionId)
                .NotEmpty().WithMessage("Condition ID is required.")
                .GreaterThan(0).WithMessage("Condition ID must be greater than 0.");

            RuleFor(x => x.ClientPOC)
                .MaximumLength(100).WithMessage("ClientPOC cannot exceed 100 characters.");

            RuleFor(x => x.SalesPOC)
                .MaximumLength(100).WithMessage("SalesPOC cannot exceed 100 characters.");

            RuleFor(x => x.Vendor)
                .NotEmpty().WithMessage("Vendor is required")
                .MaximumLength(100).WithMessage("Vendor cannot be more than 100 characters");

            RuleFor(x => x.InvoiceNumber)
                .NotEmpty().WithMessage("Invoice number is required")
                .MaximumLength(100).WithMessage("Invoice number cannot be more than 100 characters");

            RuleFor(x => x.CreatedBy)
                .GreaterThan(0).WithMessage("CreatedBy must be valid");
        }
    }
}
