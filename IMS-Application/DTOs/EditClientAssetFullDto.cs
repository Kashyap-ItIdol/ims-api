using IMS_Application.Validators;

namespace IMS_Application.DTOs
{
    public class EditClientAssetFullDto
    {
        public required string AssetName { get; set; }

        public required string Status { get; set; }

        public int CategoryId { get; set; }

        public int SubCategoryId { get; set; }

        public required string Brand { get; set; }

        public required string Model { get; set; }

        public required string SerialNumber { get; set; }

        public required string Condition { get; set; }

        public string? ItemPhoto { get; set; }

        public required string ClientName { get; set; }

        public required string ClientPOC { get; set; }

        public required string SalesPOC { get; set; }

        public required string Location { get; set; }

        public int DeskNumber { get; set; }

        public int? AssignedTo { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }

    }
}