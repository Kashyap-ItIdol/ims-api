namespace IMS_Application.DTOs
{
    public class ImportAssetsResultDto
    {
        public int TotalRows { get; set; }
        public int Inserted { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public List<ImportAssetRowErrorDto> Errors { get; set; } = new();
    }

    public class ImportAssetRowErrorDto
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

