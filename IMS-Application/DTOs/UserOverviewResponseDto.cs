using IMS_Application.DTOs;
using System.Collections.Generic;

namespace IMS_Application.DTOs
{
    public class UserOverviewResponseDto
    {
        public UserResponseDto user { get; set; } = new();
        public List<AssetResponseDto> assignedAssets { get; set; } = new();
        public List<TicketResponseDto> createdTickets { get; set; } = new();
    }
}