namespace IMS_Domain.Entities
{
    public class AssetCondition
    {
        public int Id { get; set; }
        public string Condition { get; set; } = null!;
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}