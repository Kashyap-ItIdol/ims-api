using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class CreateAndAssignAssetDtoValidator : AbstractValidator<CreateAndAssignAssetDto>
    {
        public CreateAndAssignAssetDtoValidator()
        {
            RuleFor(x => x.ItemName)
                .NotEmpty().WithMessage("Item name is required")
                .MaximumLength(100).WithMessage("Item name cannot exceed 100 characters");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category is required")
                .GreaterThan(0).WithMessage("Category ID must be greater than 0");

            RuleFor(x => x.SubCategoryId)
                .NotEmpty().WithMessage("Subcategory is required")
                .GreaterThan(0).WithMessage("SubCategory ID must be greater than 0");

            RuleFor(x => x.Brand)
                .NotEmpty().WithMessage("Brand is required")
                .MaximumLength(50).WithMessage("Brand cannot exceed 50 characters");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required")
                .MaximumLength(50).WithMessage("Model cannot exceed 50 characters");

            RuleFor(x => x.SerialNumber)
                .MaximumLength(50).WithMessage("Serial number cannot exceed 50 characters");

            RuleFor(x => x.Condition)
                .NotEmpty().WithMessage("Condition is required")
                .MaximumLength(50).WithMessage("Condition cannot exceed 50 characters");

            RuleFor(x => x.ClientPOC)
                .NotEmpty().WithMessage("Client POC is required")
                .MaximumLength(100).WithMessage("Client POC cannot exceed 100 characters");

            RuleFor(x => x.AssignedDate)
                .NotEmpty().WithMessage("Assigned date is required");

            RuleFor(x => x.ExpectedReturnDate)
                .NotEmpty().WithMessage("Expected return date is required");

            RuleFor(x => x.OfficeNo)
                .MaximumLength(20).WithMessage("Office number cannot exceed 20 characters");

            RuleFor(x => x.TableNo)
                .MaximumLength(20).WithMessage("Table number cannot exceed 20 characters");
        }
    }
}
