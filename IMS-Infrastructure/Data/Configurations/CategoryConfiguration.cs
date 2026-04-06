using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.HasData(
                new Category { Id = 1, Name = "Computing Device" },
                new Category { Id = 2, Name = "Peripherals" },
                new Category { Id = 3, Name = "Networking Equipment" },
                new Category { Id = 4, Name = "Infrastructure" },
                new Category { Id = 5, Name = "Accessories" });
        }
    }
}
