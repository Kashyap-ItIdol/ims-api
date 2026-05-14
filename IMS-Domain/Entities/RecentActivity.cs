namespace IMS_Domain.Entities
{
    public class RecentActivity
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Details { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public bool IsDeleted { get; set; } = false;
        public User? User { get; set; }
    }
}

