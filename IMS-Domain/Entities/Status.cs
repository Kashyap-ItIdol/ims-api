namespace IMS_Domain.Entities
{
    public enum Status
    {
        Open,
        InProgress,
        Solved,
        Closed,
        Conflict
    }

    public static class StatusConstants
    {
        public static readonly object[] FALLBACK_STATUSES = new[]
        {
            new { id = 1, status_id = 1, status_name = "Open", name = "Open" },
            new { id = 2, status_id = 2, status_name = "In Progress", name = "In Progress" },
            new { id = 3, status_id = 3, status_name = "Solved", name = "Solved" },
            new { id = 4, status_id = 4, status_name = "Closed", name = "Closed" },
            new { id = 5, status_id = 5, status_name = "Conflict", name = "Conflict" }
        };
    }
}
