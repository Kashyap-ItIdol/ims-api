﻿using FluentValidation;
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

        // Rule 1: Status Consistency Across Assets (with ItemName in message)
        RuleFor(x => x)
            .Custom((dto, context) =>
            {
                var assets = dto.Assets;
                if (assets == null || assets.Count < 2) return;

                var firstStatus = assets[0].StatusId;
                var mismatch = assets.FirstOrDefault(a => a.StatusId != firstStatus);
                if (mismatch != null)
                {
                    context.AddFailure($"Status mismatch: Assets \"{assets[0].ItemName}\" and \"{mismatch.ItemName}\" must have the same status.");
                }
            });

        // Rule 2: Assignment Required When Status = 2
        When(x => x.Assets.FirstOrDefault(a => a.IsPrimary)?.StatusId == 2, () =>
        {
            RuleFor(x => x.AssignedTo)
                .NotNull().WithMessage(ErrorMessages.AssignmentDetailsRequiredWhenAssigned);

            RuleFor(x => x.AssignedDate)
                .NotNull().WithMessage(ErrorMessages.AssignmentDetailsRequiredWhenAssigned);
        });

        // Rule 3: Assignment Not Allowed When Status ≠ 2
        When(x => x.Assets.FirstOrDefault(a => a.IsPrimary)?.StatusId != 2, () =>
        {
            RuleFor(x => x.AssignedTo)
                .Null().WithMessage(ErrorMessages.AssignmentDetailsNotAllowedWhenNotAssigned);

            RuleFor(x => x.AssignedDate)
                .Null().WithMessage(ErrorMessages.AssignmentDetailsNotAllowedWhenNotAssigned);

            RuleFor(x => x.Location)
                .Null().WithMessage(ErrorMessages.AssignmentDetailsNotAllowedWhenNotAssigned);

            RuleFor(x => x.TableNo)
                .Null().WithMessage(ErrorMessages.AssignmentDetailsNotAllowedWhenNotAssigned);
        });

        // Assignment validation (when assignment is present)
        When(x => x.AssignedTo.HasValue, () =>
        {
            RuleFor(x => x.TableNo)
                .NotEmpty().WithMessage(ErrorMessages.InvalidTableNo);

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage(ErrorMessages.LocationRequired);

            RuleFor(x => x)
                .Must(x =>
                {
                    var primary = x.Assets.FirstOrDefault(a => a.IsPrimary);
                    return primary?.StatusId == 2;
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

