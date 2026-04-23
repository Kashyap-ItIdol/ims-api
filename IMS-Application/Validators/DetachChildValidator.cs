using FluentValidation;
using IMS_Application.DTOs;

namespace IMS_Application.Validators
{
    public class DetachChildValidator : AbstractValidator<DetachChildDto>
    {
        public DetachChildValidator()
        {
            RuleFor(x => x.ChildId)
                .GreaterThan(0).WithMessage("Child ID must be greater than 0");

            RuleFor(x => x.ParentId)
                .GreaterThan(0).WithMessage("Parent ID must be greater than 0");
        }
    }
}
