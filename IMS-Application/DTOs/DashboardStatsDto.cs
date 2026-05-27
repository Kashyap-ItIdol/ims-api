using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Application.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalAssets { get; set; }

        public int AvailableAssets { get; set; }

        public int OpenTickets { get; set; }

        public int InProgressTickets { get; set; }

        public int ResolvedTickets { get; set; }

        public double TotalAssetsPercentage { get; set; }
        public string TotalAssetsTrend { get; set; } = "down";
        public double TotalAssetsChangePercentage { get; set; }
        public string TotalAssetsComparisonText { get; set; } = string.Empty;

        public double AvailableAssetsPercentage { get; set; }
        public string AvailableAssetsTrend { get; set; } = "down";
        public double AvailableAssetsChangePercentage { get; set; }
        public string AvailableAssetsComparisonText { get; set; } = string.Empty;

        public double OpenTicketsPercentage { get; set; }
        public string OpenTicketsTrend { get; set; } = "down";
        public double OpenTicketsChangePercentage { get; set; }
        public string OpenTicketsComparisonText { get; set; } = string.Empty;

        public double InProgressTicketsPercentage { get; set; }
        public string InProgressTicketsTrend { get; set; } = "down";
        public double InProgressTicketsChangePercentage { get; set; }
        public string InProgressTicketsComparisonText { get; set; } = string.Empty;

        public double ResolvedTicketsPercentage { get; set; }
        public string ResolvedTicketsTrend { get; set; } = "down";
        public double ClosedTicketsChangePercentage { get; set; }
        public string ClosedTicketsComparisonText { get; set; } = string.Empty;
    }

}
