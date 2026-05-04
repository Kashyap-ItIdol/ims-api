using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class EditClientAssetQuickDtoValidator : AbstractValidator<EditClientAssetQuickDto>
    {
        public EditClientAssetQuickDtoValidator()
        {
            RuleFor(x => x.AssetName)
                .Length(2, 100).When(x => !string.IsNullOrEmpty(x.AssetName))
                .WithMessage("Asset name must be between 2 and 100 characters");

            RuleFor(x => x.SerialNumber)
                .NotEmpty().When(x => !string.IsNullOrEmpty(x.SerialNumber))
                .WithMessage("Serial number cannot be empty when provided");
        }
    }
}
