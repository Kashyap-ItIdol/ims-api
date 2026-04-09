namespace IMS_Domain.Entities
{
    public class Department
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}