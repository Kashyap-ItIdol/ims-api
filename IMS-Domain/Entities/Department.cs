using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IMS_Domain.Entities
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }
    }
}
