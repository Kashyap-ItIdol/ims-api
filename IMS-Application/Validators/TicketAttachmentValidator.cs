using FluentValidation;
using IMS_Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace IMS_Application.Validators
{
    public class TicketAttachmentValidator : AbstractValidator<TicketAttachmentRequestDto>
    {
        public TicketAttachmentValidator()
        {
            RuleFor(x => x.Files)
                .Must(files => files == null || files.Any())
                .WithMessage("If files are provided, at least one file must be included.");

            When(x => x.Files != null && x.Files.Any(), () => {
                RuleForEach(x => x.Files)
                    .Must(file => file != null && file.Length > 0)
                    .WithMessage("File cannot be empty.")
                    .Must(file => IsAllowedFileType(file))
                    .WithMessage("Only PNG, JPG, and JPEG files are allowed.")
                    .Must(file => file.Length <= 5 * 1024 * 1024) // 5MB limit
                    .WithMessage("File size cannot exceed 5MB.");
            });
        }

        private bool IsAllowedFileType(IFormFile file)
        {
            if (file == null || string.IsNullOrEmpty(file.FileName))
                return false;

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            return allowedExtensions.Contains(fileExtension);
        }
    }
}
