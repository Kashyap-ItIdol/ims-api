namespace IMS_Domain.Entities
{
    public class NetworkDetail
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string IPAddress { get; set; } = null!;
        public string MacAddress { get; set; } = null!;
        public string Hostname { get; set; } = null!;
        public string SubnetMask { get; set; } = null!;
        public string Gateway { get; set; } = null!;
        public string DNS { get; set; } = null!;
        public string createdBy { get; set; } = null!;
        public string updatedBy { get; set; } = null!;
    }
}
