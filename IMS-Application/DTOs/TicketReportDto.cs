namespace IMS_Application.DTOs
{
    public class TicketReportDto
    {
        public ReportKpiDto TotalTicketsAssignmentKpi { get; set; }
        public ReportKpiDto TotalTicketsSolvedKpi { get; set; }
        public ReportKpiDto TotalTicketsReopenedKpi { get; set; }
        public ReportKpiDto TotalTicketsConflictHandledKpi { get; set; }
        public Dictionary<string, DayActivityDto> TicketActivityOverview { get; set; }
        public PriorityDistributionDto PriorityDistribution { get; set; }
        public TicketTypeAnalysisDto TicketTypeAnalysis { get; set; }
        public Dictionary<string, EngineerStageDto> TicketPerSupportEngineer { get; set; }
        public ReportMetricsDto Metrics { get; set; }
    }

    public class ReportKpiDto
    {
        public int TotalCount { get; set; }
        public PreviousPeriodDto PreviousPeriod { get; set; }
        public int CriticalCount { get; set; }
        public string Message { get; set; }
    }

    public class DayActivityDto
    {
        public int Assigned { get; set; }
        public int Solved { get; set; }
    }

    public class PriorityDistributionDto
    {
        public PriorityItemDto Critical { get; set; }
        public PriorityItemDto High { get; set; }
        public PriorityItemDto Medium { get; set; }
        public PriorityItemDto Low { get; set; }
    }

    public class PriorityItemDto
    {
        public int Count { get; set; }
        public int Percentage { get; set; }
    }

    public class TicketTypeAnalysisDto
    {
        public int Hardware { get; set; }
        public int Software { get; set; }
        public int Website { get; set; }
        public int Server { get; set; }
        public int Other { get; set; }
    }

    public class EngineerStageDto
    {
        public Dictionary<string, int> Engineers { get; set; }
    }

    public class PreviousPeriodDto
    {
        public int Count { get; set; }
        public int PercentageChange { get; set; }
        public string Trend { get; set; }
        public string Message { get; set; }
    }

    public class ReportMetricsDto
    {
        public double AvgResolutionPerDay { get; set; }
        public int TicketsResolvedPerDay { get; set; }
        public string AvgFirstResponseTime { get; set; }
    }
}