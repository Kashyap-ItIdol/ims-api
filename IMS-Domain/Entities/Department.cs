using System.ComponentModel.DataAnnotations;

namespace IMS_Domain.Entities
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }
    }
}
