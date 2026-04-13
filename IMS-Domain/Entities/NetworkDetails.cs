namespace IMS_Domain.Entities
{
    public class NetworkDetails
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string IPAddress { get; set; }
        public string MacAddress { get; set; }
        public string Hostname { get; set; }
        public string SubnetMask { get; set; }
        public string Gateway { get; set; }
        public string DNS { get; set; }
        public string createdBy { get; set; }
        public string updatedBy { get; set; }
    }
}
