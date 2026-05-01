using FluentValidation;
using IMS_Application.Common.Constants;
using IMS_Application.DTOs;
using IMS_Domain.Entities;

namespace IMS_Application.Validators
{
    public class UpdateTicketDtoValidator : AbstractValidator<UpdateTicketDto>
    {
        public UpdateTicketDtoValidator()
        {
            RuleFor(x => x.TicketTitle)
                .MaximumLength(500).WithMessage("Title cannot exceed 500 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.TicketTitle));

            When(x => !string.IsNullOrWhiteSpace(x.TicketType), () =>
            {
                RuleFor(x => x.TicketType)
                    .Must(t => !string.IsNullOrWhiteSpace(t) && Enum.TryParse<TicketType>(t, true, out _))
                    .WithMessage(ErrorMessages.InvalidTicketType);
            });

            When(x => !string.IsNullOrWhiteSpace(x.TicketPriority), () =>
            {
                RuleFor(x => x.TicketPriority)
                    .Must(p => !string.IsNullOrWhiteSpace(p) && Enum.TryParse<TicketPriority>(p, true, out _))
                    .WithMessage(ErrorMessages.InvalidTicketPriority);
            });

            RuleFor(x => x.AssetId)
                .GreaterThan(0).WithMessage("Asset ID must be greater than 0")
                .When(x => x.AssetId.HasValue);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category ID must be greater than 0")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.SubCategoryId)
                .GreaterThan(0).WithMessage("SubCategory ID must be greater than 0")
                .When(x => x.SubCategoryId.HasValue);

            RuleFor(x => x)
                .Must(x => !x.SubCategoryId.HasValue || !x.CategoryId.HasValue || 
                    (x.SubCategoryId.HasValue && x.CategoryId.HasValue))
                .WithMessage(ErrorMessages.SubCategoryCategoryIdInvalid)
                .When(x => x.SubCategoryId.HasValue && x.CategoryId.HasValue);
        }
    }
}
