using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles"); // Specify the table name

            builder.HasKey(r => r.Id); // Define the primary key

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Support Engineer" },
                new Role { Id = 3, Name = "Employee" }
            );
        }
    }
}