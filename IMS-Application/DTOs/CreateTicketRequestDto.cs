using System.ComponentModel.DataAnnotations;

namespace IMS_Application.DTOs
{
    public class CreateTicketRequestDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int TypeId { get; set; }

        public int PriorityId { get; set; }

        public int? AssetId { get; set; }

        public int assignedTo { get; set; }
    }
}
