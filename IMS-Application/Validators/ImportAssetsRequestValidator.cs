using FluentValidation;
using IMS_Application.DTOs;
using IMS_Application.Common.Constants;
using Microsoft.AspNetCore.Http;

namespace IMS_Application.Validators
{
    public class ImportAssetsRequestValidator : AbstractValidator<ImportAssetsRequestDto>
    {
        private const long MaxCsvFileSizeBytes = 10 * 1024 * 1024;

        public ImportAssetsRequestValidator()
        {
            RuleFor(x => x.CsvFile)
                .NotNull()
                .WithMessage("CSV file is required")
                .DependentRules(() =>
                {
                    RuleFor(x => x.CsvFile!.Length)
                        .GreaterThan(0)
                        .WithMessage("CSV file cannot be empty");

                    RuleFor(x => x.CsvFile!.Length)
                        .LessThanOrEqualTo(MaxCsvFileSizeBytes)
                        .WithMessage($"CSV file size cannot exceed {MaxCsvFileSizeBytes / (1024 * 1024)}MB");

                    RuleFor(x => x.CsvFile!.FileName)
                        .NotEmpty()
                        .WithMessage("CSV file name is required");

                    RuleFor(x => x.CsvFile!.FileName)
                        .Must(BeCsvFile)
                        .WithMessage("Only .csv files are allowed");

                    RuleFor(x => x.CsvFile)
                        .Must(HaveValidContentType)
                        .WithMessage("Invalid CSV content type");
                });
        }

        private static bool BeCsvFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            return fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
        }

        private static bool HaveValidContentType(IFormFile? file)
        {
            if (file == null)
                return true;

            if (string.IsNullOrWhiteSpace(file.ContentType))
                return true;

            var contentType = file.ContentType.Trim().ToLowerInvariant();
            return contentType is "text/csv" or "application/csv" or "application/vnd.ms-excel";
        }
    }
}

