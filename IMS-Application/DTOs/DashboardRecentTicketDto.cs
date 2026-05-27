using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class RecentTicketDto
    {
        public string TicketId { get; set; } = string.Empty;

        public string RaisedBy { get; set; } = string.Empty;

        public string IssueType { get; set; } = string.Empty;

        public string AssignedTo { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;
    }
}
