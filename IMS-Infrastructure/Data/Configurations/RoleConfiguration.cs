using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Employee" },
                new Role { Id = 3, Name = "Support Engineer" }
            );
        }
    }
}
