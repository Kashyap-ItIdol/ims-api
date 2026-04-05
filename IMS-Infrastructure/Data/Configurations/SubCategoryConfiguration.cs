using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class SubCategoryConfiguration : IEntityTypeConfiguration<SubCategory>
    {
        public void Configure(EntityTypeBuilder<SubCategory> builder)
        {

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            // Relationship with Category
            builder.HasOne(s => s.Category)
                   .WithMany(c => c.SubCategory)
                   .HasForeignKey(s => s.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(
                new SubCategory { Id = 1, Name = "Laptop", CategoryId = 1 },
                new SubCategory { Id = 2, Name = "CPU", CategoryId = 1 },
                new SubCategory { Id = 3, Name = "Mouse", CategoryId = 2 },
                new SubCategory { Id = 4, Name = "Keyboard", CategoryId = 2 },
                new SubCategory { Id = 5, Name = "WiFi Router", CategoryId = 3 },
                new SubCategory { Id = 6, Name = "Firewall", CategoryId = 3 },
                new SubCategory { Id = 7, Name = "Server", CategoryId = 4 },
                new SubCategory { Id = 8, Name = "UPS", CategoryId = 4 },
                new SubCategory { Id = 9, Name = "Charger", CategoryId = 5 });
        }
    }
}
