using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class AddAssetValidator : AbstractValidator<AddAssetDto>
    {
        public AddAssetValidator()
        {
            RuleFor(x => x.Assets)
                .NotEmpty().WithMessage("Assets list cannot be empty");

            RuleFor(x => x.Assets.Count(a => a.IsPrimary))
                .Equal(1).WithMessage("Exactly one primary asset required");

            RuleForEach(x => x.Assets).ChildRules(asset =>
            {
                asset.RuleFor(a => a.SerialNo).NotEmpty();
                asset.RuleFor(a => a.ItemName).NotEmpty();
            });
        }
    }
}
