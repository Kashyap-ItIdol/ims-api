using FluentValidation;
using IMS_Application.Common.Constants;
using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Validators
{
    public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequestDto>
    {
        public CreateTicketRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(500).WithMessage("Title cannot exceed 500 characters");

            RuleFor(x => x.Priority)
                .NotEmpty().WithMessage("Priority is required")
                .Must(p => Enum.TryParse<TicketPriority>(p, true, out _)).WithMessage(ErrorMessages.InvalidTicketPriority);

            RuleFor(x => x.assignedTo)
                .GreaterThan(0).WithMessage("Assigned user is required");

            RuleFor(x => x)
                .Must(x => x.AssetId.HasValue || (x.CategoryId.HasValue && x.SubCategoryId.HasValue))
                .WithMessage("Either AssetId or both CategoryId and SubCategoryId are required.");

            RuleFor(x => x.TicketType)
                .NotEmpty().WithMessage("TicketType is required for existing assets")
                .Must(t => Enum.TryParse<TicketType>(t, true, out _))
                .WithMessage(ErrorMessages.InvalidTicketType)
                .When(x => x.AssetId.HasValue);

        }
    }
}
