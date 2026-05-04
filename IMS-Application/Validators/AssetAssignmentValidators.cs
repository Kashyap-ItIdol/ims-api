using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class AssetAssignmentDtoValidator : AbstractValidator<AssetAssignmentDto>
    {
        public AssetAssignmentDtoValidator()
        {
            RuleFor(x => x.AssetId)
                .GreaterThan(0).WithMessage("Asset ID is required and must be greater than 0");

            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Employee ID is required and must be greater than 0");

            RuleFor(x => x.AssignedDate)
                .NotEmpty().WithMessage("Assigned date is required")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Assigned date cannot be in the future");

            RuleFor(x => x.ExpectedReturnDate)
                .GreaterThanOrEqualTo(x => x.AssignedDate).WithMessage("Expected return date must be on or after assigned date");

            RuleFor(x => x.OfficeNo)
                .MaximumLength(20).WithMessage("Office number cannot exceed 20 characters");

            RuleFor(x => x.TableNo)
                .MaximumLength(20).WithMessage("Table number cannot exceed 20 characters");
        }
    }

    public class UpdateAssetAssignmentDtoValidator : AbstractValidator<AssetAssignmentDto>
    {
        public UpdateAssetAssignmentDtoValidator()
        {
            RuleFor(x => x.AssetId)
                .GreaterThan(0).WithMessage("Asset ID is required and must be greater than 0");

            RuleFor(x => x.EmployeeId)
                .GreaterThan(0).WithMessage("Employee ID is required and must be greater than 0");

            RuleFor(x => x.AssignedDate)
                .NotEmpty().WithMessage("Assigned date is required");

            RuleFor(x => x.ExpectedReturnDate)
                .GreaterThanOrEqualTo(x => x.AssignedDate).WithMessage("Expected return date must be on or after assigned date");

            RuleFor(x => x.OfficeNo)
                .MaximumLength(20).WithMessage("Office number cannot exceed 20 characters");

            RuleFor(x => x.TableNo)
                .MaximumLength(20).WithMessage("Table number cannot exceed 20 characters");
        }
    }
}
