namespace IMS_Domain.Entities
{
    public class Role : BaseEntity
    {
     //   public int Id { get; set; }

        public required string Name { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
