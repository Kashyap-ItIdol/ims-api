using FluentValidation;
using IMS_Application.DTOs;
using IMS_Application.Common.Constants;

namespace IMS_Application.Validators
{
    public class AssignAssetValidator : AbstractValidator<AssignAssetDto>
    {
        public AssignAssetValidator()
        {
            RuleFor(x => x.AssetId)
                .GreaterThan(0).WithMessage("Asset ID must be greater than 0");

            RuleFor(x => x.UserId)
                .GreaterThan(0).WithMessage(ErrorMessages.UserNotFound);

            RuleFor(x => x.AssignedDate)
                .LessThanOrEqualTo(DateTime.UtcNow);

            When(x => x.ExpectedReturnDate.HasValue, () =>
            {
                RuleFor(x => x.ExpectedReturnDate)
                    .GreaterThanOrEqualTo(x => x.AssignedDate).WithMessage("ExpectedReturnDate cannot be before AssignedDate");
            });

            RuleFor(x => x.Location)
                .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Location));

            RuleFor(x => x.TableNo)
                .MaximumLength(50).When(x => !string.IsNullOrEmpty(x.TableNo));
        }
    }
}
