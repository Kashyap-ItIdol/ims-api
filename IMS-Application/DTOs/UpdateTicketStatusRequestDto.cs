using System.ComponentModel.DataAnnotations;

namespace IMS_Application.DTOs
{
    public class UpdateTicketStatusRequestDto
    {
        public int TicketId { get; set; }

        public int StatusId { get; set; }
    }
}
