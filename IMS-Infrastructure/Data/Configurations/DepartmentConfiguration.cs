using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasData(
                new Department { Id = 1, Name = "Sales" },
                new Department { Id = 2, Name = "Marketing" },
                new Department { Id = 3, Name = "Designing" },
                new Department { Id = 4, Name = "Accounts" },
                new Department { Id = 5, Name = "Developers" }
                //new Department { Id = 6, Name = "DevOps" },
                //new Department { Id = 7, Name = "UI/UX" },
                //new Department { Id = 8, Name = "Mobile App" }
            );
        }
    }
}
