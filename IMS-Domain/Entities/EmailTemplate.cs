namespace IMS_Domain.Entities
{
    public class EmailTemplate
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Subject { get; set; } = null!;

        public string BodyHtml { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
