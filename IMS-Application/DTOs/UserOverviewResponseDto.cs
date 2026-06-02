namespace IMS_Application.DTOs
{
    public class UserOverviewResponseDto
    {
        public UserResponseDto User { get; set; } = new();
        public List<AssetResponseDto> AssignedAssets { get; set; } = new();
        public List<TicketResponseDto> CreatedTickets { get; set; } = new();
    }
}