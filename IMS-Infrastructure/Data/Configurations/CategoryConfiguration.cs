using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // Table
            builder.ToTable("Categories");

            // PK
            builder.HasKey(c => c.Id);

            // Properties
            builder.Property(c => c.CategoryName)
                   .IsRequired()
                   .HasMaxLength(100);

            // 🔥 Relationship: Category → SubCategories (1:N)
            builder.HasMany(c => c.SubCategories)
                   .WithOne(sc => sc.Category)
                   .HasForeignKey(sc => sc.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 🔥 Relationship: Category → Inventory (1:N)
            builder.HasMany(c => c.Inventory)
                   .WithOne(i => i.Category)
                   .HasForeignKey(i => i.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}