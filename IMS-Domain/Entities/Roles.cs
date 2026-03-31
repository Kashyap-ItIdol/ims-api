using System.ComponentModel.DataAnnotations;

namespace IMS_Domain.Entities
{
    public class Roles
    {
        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }
    }
}
