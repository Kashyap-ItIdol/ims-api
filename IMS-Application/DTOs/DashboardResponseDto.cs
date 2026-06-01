using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class DashboardResponseDto
    {
        public DashboardStatsDto Stats { get; set; }

        public List<TicketActivityDto> TicketActivity { get; set; }

        public TicketConflictStatusDto TicketConflictStatus { get; set; }

        public List<AssetDistributionItemDto> AssetDistribution { get; set; }

        public List<RecentActivityDto> RecentActivities { get; set; }

        public List<AttentionItemDto> RequiresAttention { get; set; }

        public List<DeletedItemDto> RecentlyDeleted { get; set; }
    }


    public class TicketActivityDto
    {
        public string Day { get; set; } = string.Empty;

        public int Opened { get; set; }

        public int Resolved { get; set; }
    }

    public class TicketConflictStatusDto
    {
        public int Critical { get; set; }

        public int High { get; set; }

        public int Medium { get; set; }

        public int Low { get; set; }
    }

    public class AssetDistributionItemDto
    {
        public string Label { get; set; } = string.Empty;

        public int Value { get; set; }
    }
}






