namespace IMS_Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
