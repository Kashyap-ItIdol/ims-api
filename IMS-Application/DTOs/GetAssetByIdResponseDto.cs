namespace IMS_Application.DTOs
{
    public class GetAssetByIdResponseDto
    {
        public bool IsParent { get; set; }

        public AssetOverviewDto Overview { get; set; } = null!;
        public AssetAssignmentDto Assignment { get; set; } = null!;
    }
}
