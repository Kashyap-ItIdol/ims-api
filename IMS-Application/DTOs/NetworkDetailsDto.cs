namespace IMS_Application.DTOs
{
    public class NetworkDetailsDto
    {
        public string IPAddress { get; set; } = null!;
        public string MacAddress { get; set; } = null!;
        public string Hostname { get; set; } = null!;
        public string SubnetMask { get; set; } = null!;
        public string Gateway { get; set; } = null!;
        public string DNS { get; set; } = null!;
    }
}
