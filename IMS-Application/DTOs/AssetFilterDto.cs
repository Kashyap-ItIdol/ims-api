namespace IMS_Application.DTOs
{
    public class AssetFilterDto
    {
        public List<int>? CategoryIds { get; set; }
        public List<int>? SubCategoryIds { get; set; }
        public List<int>? StatusIds { get; set; }
        public string? Search { get; set; }
        public string? SearchType { get; set; }
        // values: "category", "subcategory", "status"
    }
}
