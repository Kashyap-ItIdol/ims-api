namespace IMS_Domain.Entities
{
    public class AssetStatus
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}