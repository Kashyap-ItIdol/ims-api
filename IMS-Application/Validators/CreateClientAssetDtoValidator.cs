using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class CreateClientAssetDtoValidator : AbstractValidator<CreateClientAssetDto>
    {
        public CreateClientAssetDtoValidator()
        {
            RuleFor(x => x.AssetName)
                .NotEmpty().WithMessage("Asset name is required")
                .Length(2, 100).WithMessage("Asset name must be between 2 and 100 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required");

            RuleFor(x => x.AssetId)
                .NotEmpty().WithMessage("Asset ID is required")
                .GreaterThan(0).WithMessage("Asset ID must be greater than 0");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID is required")
                .GreaterThan(0).WithMessage("Category ID must be greater than 0");

            RuleFor(x => x.SubCategoryId)
                .NotEmpty().WithMessage("SubCategory ID is required")
                .GreaterThan(0).WithMessage("SubCategory ID must be greater than 0");

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Brand is required");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required");

            RuleFor(x => x.SerialNumber)
                .NotEmpty().WithMessage("Serial number is required");

            RuleFor(x => x.Condition)
                .NotEmpty().WithMessage("Condition is required");

            RuleFor(x => x.ClientName)
                .NotEmpty().WithMessage("Client name is required");

            RuleFor(x => x.ClientPOC)
                .NotEmpty().WithMessage("Client POC is required");

            RuleFor(x => x.SalesPOC)
                .NotEmpty().WithMessage("Sales POC is required");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required");
        }
    }
}
