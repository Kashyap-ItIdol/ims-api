using IMS_Domain.Entities;

namespace IMS_Domain.Entities
{
    public class AssetHistory
    {
        public int Id { get; set; }

        public int AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        public string Action { get; set; } = null!;
        // e.g. "Assigned", "Unassigned", "Created", "Updated", "Attached", "Detached"

        public string Description { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? CreatedBy { get; set; }
    }
}
