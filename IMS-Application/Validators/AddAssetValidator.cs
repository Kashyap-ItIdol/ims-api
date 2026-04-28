using FluentValidation;
using IMS_Application.Common.Constants;
using IMS_Application.DTOs;

public class AddAssetValidator : AbstractValidator<AddAssetDto>
{
    public AddAssetValidator()
    {
        // Assets required
        RuleFor(x => x.Assets)
            .NotEmpty().WithMessage(ErrorMessages.AssetsListEmpty);

        // Only one primary
        RuleFor(x => x.Assets.Count(a => a.IsPrimary))
            .Equal(1).WithMessage(ErrorMessages.ExactlyOnePrimaryAssetRequired);

        RuleForEach(x => x.Assets).ChildRules(asset =>
        {
            asset.RuleFor(a => a.SerialNo)
                .NotEmpty().WithMessage(ErrorMessages.SerialNumberRequired);

            asset.RuleFor(a => a.ItemName)
                .NotEmpty().WithMessage(ErrorMessages.ItemNameRequired)
                .MaximumLength(200).WithMessage(ErrorMessages.InvalidStringLength);

            asset.RuleFor(a => a.CategoryId)
                .GreaterThan(0).WithMessage(ErrorMessages.InvalidCategoryId);

            asset.RuleFor(a => a.SubCategoryId)
                .GreaterThan(0).WithMessage(ErrorMessages.InvalidSubCategoryId);

            asset.RuleFor(a => a.ConditionId)
                .GreaterThan(0).WithMessage(ErrorMessages.InvalidConditionId);

            asset.RuleFor(a => a.StatusId)
                .GreaterThan(0).WithMessage("Status is required");

            // Purchase validation
            asset.When(a => !a.IsPurchaseDetailsSame, () =>
            {
                asset.RuleFor(a => a.Vendor)
                    .NotEmpty().WithMessage(ErrorMessages.VendorRequired);

                asset.RuleFor(a => a.PurchaseCost)
                    .GreaterThan(0).WithMessage(ErrorMessages.InvalidPurchaseCost);

                asset.RuleFor(a => a.PurchaseDate)
                    .LessThanOrEqualTo(DateTime.UtcNow)
                    .WithMessage(ErrorMessages.InvalidPurchaseDate);

                asset.RuleFor(a => a.InvoiceNumber)
                    .NotEmpty().WithMessage(ErrorMessages.InvoiceNumberRequired);
            });
        });

        // Assignment validation
        When(x => x.AssignedTo.HasValue, () =>
        {
            RuleFor(x => x.TableNo)
                .NotEmpty().WithMessage(ErrorMessages.InvalidTableNo);

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage(ErrorMessages.LocationRequired);

            // 🔥 Important business rule
            RuleFor(x => x)
                .Must(x =>
                {
                    var primary = x.Assets.First(a => a.IsPrimary);
                    return primary.StatusId == 2; // Assigned
                })
                .WithMessage(ErrorMessages.AssignedAssetMustBeAssigned);
        });

        // Date validation
        When(x => x.AssignedDate.HasValue && x.ExpectedReturnDate.HasValue, () =>
        {
            RuleFor(x => x.ExpectedReturnDate)
                .GreaterThanOrEqualTo(x => x.AssignedDate!.Value)
                .WithMessage(ErrorMessages.InvalidDateRange);
        });
    }
}