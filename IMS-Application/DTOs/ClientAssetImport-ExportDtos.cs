using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace IMS_Application.DTOs
{
    public class ClientAssetCsvImportRequestDto
    {
        public IFormFile File { get; set; } = default!;
        public bool HasHeaderRow { get; set; } = true;
    }

    public class ClientAssetCsvImportResultDto
    {
        public int TotalRows { get; set; }
        public int ImportedRows { get; set; }
        public int FailedRows { get; set; }
        public IReadOnlyList<CsvRowErrorDto> Errors { get; set; } = new List<CsvRowErrorDto>();
    }

    public class CsvRowErrorDto
    {
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}