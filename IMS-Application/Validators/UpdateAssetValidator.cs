using FluentValidation;
using IMS_Application.DTOs;
using IMS_Application.Common.Constants;

namespace IMS_Application.Validators
{
    public class UpdateAssetValidator : AbstractValidator<UpdateAssetDto>
    {
        public UpdateAssetValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Asset ID must be greater than 0");

            RuleFor(x => x.ItemName)
                .NotEmpty().WithMessage(ErrorMessages.ItemNameRequired);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage(ErrorMessages.InvalidCategoryId);

            RuleFor(x => x.SubCategoryId)
                .GreaterThan(0).WithMessage(ErrorMessages.InvalidSubCategoryId);

            RuleFor(x => x.ConditionId)
                .GreaterThan(0).WithMessage(ErrorMessages.InvalidConditionId);

            RuleFor(x => x.StatusId)
                .GreaterThan(0);

            RuleFor(x => x.SerialNo)
                .NotEmpty().WithMessage(ErrorMessages.SerialNumberRequired)
                .MaximumLength(100);

            RuleFor(x => x.Brand)
                .NotEmpty().MaximumLength(100);

            RuleFor(x => x.Model)
                .NotEmpty().MaximumLength(100);

            RuleFor(x => x.Vendor)
                .NotEmpty().WithMessage(ErrorMessages.VendorRequired)
                .MaximumLength(200);

            RuleFor(x => x.PurchaseCost)
                .GreaterThan(0).WithMessage(ErrorMessages.PurchaseCostInvalid);

            RuleFor(x => x.PurchaseDate)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage(ErrorMessages.PurchaseDateRequired);

            When(x => x.AssignedDate.HasValue && x.ExpectedReturnDate.HasValue, () =>
            {
                RuleFor(x => x.ExpectedReturnDate)
                    .GreaterThanOrEqualTo(x => x.AssignedDate.Value).WithMessage("ExpectedReturnDate cannot be before AssignedDate");
            });
        }
    }
}
