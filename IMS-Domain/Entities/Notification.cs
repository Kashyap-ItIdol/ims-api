namespace IMS_Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public required string Title { get; set; }

        public required string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }
    }
}
