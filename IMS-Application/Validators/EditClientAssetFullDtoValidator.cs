using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class EditClientAssetFullDtoValidator : AbstractValidator<EditClientAssetFullDto>
    {
        public EditClientAssetFullDtoValidator()
        {
            RuleFor(x => x.AssetName)
                .NotEmpty().WithMessage("Asset name is required")
                .Length(2, 100).WithMessage("Asset name must be between 2 and 100 characters");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(status => status == "Assigned" || status == "Returned")
                .WithMessage("Status must be either 'Assigned' or 'Returned'.");

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Brand is required");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required");

            RuleFor(x => x.SerialNumber)
                .NotEmpty().WithMessage("Serial number is required");

            RuleFor(x => x.Condition)
                .NotEmpty().WithMessage("Condition is required")
                .Must(condition => condition == "Good" || condition == "Bad")
                .WithMessage("Condition must be either 'Good' or 'Bad'.");

            RuleFor(x => x.ClientName)
                .NotEmpty().WithMessage("Client name is required");

            RuleFor(x => x.ClientPOC)
                .NotEmpty().WithMessage("Client POC is required");

            RuleFor(x => x.SalesPOC)
                .NotEmpty().WithMessage("Sales POC is required");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than 0");

            RuleFor(x => x.SubCategoryId)
                .GreaterThan(0).WithMessage("SubCategory ID must be greater than 0");

            RuleFor(x => x.DeskNumber)
                .GreaterThan(0).WithMessage("Desk number must be greater than 0");
        }
    }
}
