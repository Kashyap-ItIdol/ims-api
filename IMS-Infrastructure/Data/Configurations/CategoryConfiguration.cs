using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(x => x.CategoryName).IsUnique();
            builder.HasMany(c => c.SubCategories)
                .WithOne(sc => sc.Category)
                .HasForeignKey(sc => sc.CategoryId);

            builder.HasData(
    new Category { Id = 1, CategoryName = "Laptop" },
    new Category { Id = 2, CategoryName = "Desktop" },
    new Category { Id = 3, CategoryName = "Accessories" }
);
        }
    }
}