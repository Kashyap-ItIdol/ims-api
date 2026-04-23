using FluentValidation;
using IMS_Application.Common.Constants;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class AddAssetValidator : AbstractValidator<AddAssetDto>
    {
        public AddAssetValidator()
        {
            RuleFor(x => x.Assets)
                .NotEmpty().WithMessage(ErrorMessages.AssetsListEmpty);

            RuleFor(x => x.Assets.Count(a => a.IsPrimary))
                .Equal(1).WithMessage(ErrorMessages.ExactlyOnePrimaryAssetRequired);

            RuleForEach(x => x.Assets).ChildRules(asset =>
            {
                asset.RuleFor(a => a.SerialNo)
                    .NotEmpty().WithMessage(ErrorMessages.SerialNumberRequired)
                    .WithMessage(ErrorMessages.InvalidSerialFormat);

                asset.RuleFor(a => a.ItemName)
                    .NotEmpty().WithMessage(ErrorMessages.ItemNameRequired)
                    .Length(1, 200).WithMessage(ErrorMessages.InvalidStringLength);

                asset.RuleFor(a => a.CategoryId)
                    .GreaterThan(0).WithMessage(ErrorMessages.InvalidCategoryId);

                asset.RuleFor(a => a.SubCategoryId)
                    .GreaterThan(0).WithMessage(ErrorMessages.InvalidSubCategoryId);

                asset.RuleFor(a => a.ConditionId)
                    .GreaterThan(0).WithMessage(ErrorMessages.InvalidConditionId);

                asset.RuleFor(a => a.IsPurchaseDetailsSame)
                    .NotNull().WithMessage("IsPurchaseDetailsSame is required");

                asset.RuleFor(a => a.Vendor)
                    .NotEmpty().When(a => !a.IsPurchaseDetailsSame).WithMessage(ErrorMessages.VendorRequired)
                    .Length(1, 200).When(a => !a.IsPurchaseDetailsSame).WithMessage(ErrorMessages.InvalidStringLength);

                asset.RuleFor(a => a.PurchaseCost)
                    .GreaterThan(0).LessThanOrEqualTo(1000000m).When(a => !a.IsPurchaseDetailsSame).WithMessage(ErrorMessages.PurchaseCostInvalid)
                    .WithMessage(ErrorMessages.InvalidPurchaseCost);

                asset.RuleFor(a => a.PurchaseDate)
                    .NotNull().LessThanOrEqualTo(DateTime.UtcNow).GreaterThan(DateTime.MinValue.AddYears(100))
                    .When(a => !a.IsPurchaseDetailsSame).WithMessage(ErrorMessages.PurchaseDateRequired)
                    .WithMessage(ErrorMessages.InvalidPurchaseDate);

                asset.RuleFor(a => a.Brand)
                    .Length(1, 200).When(a => !string.IsNullOrEmpty(a.Brand)).WithMessage(ErrorMessages.InvalidStringLength);
            });

            
            RuleFor(x => x.TableNo)
                .NotEmpty().WithMessage(ErrorMessages.InvalidTableNo);

            When(x => x.AssignedDate.HasValue && x.ExpectedReturnDate.HasValue, () =>
                RuleFor(x => x.ExpectedReturnDate)
                    .GreaterThanOrEqualTo(x => x.AssignedDate.Value)
                    .WithMessage(ErrorMessages.InvalidDateRange)
            );
        }
    }
}