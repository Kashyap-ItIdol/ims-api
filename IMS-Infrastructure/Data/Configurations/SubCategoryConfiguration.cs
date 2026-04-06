using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Configurations
{
    public class SubCategoryConfiguration : IEntityTypeConfiguration<SubCategory>
    {
        public void Configure(EntityTypeBuilder<SubCategory> builder)
        {
            // Table
            builder.ToTable("SubCategories");

            // PK
            builder.HasKey(sc => sc.Id);

            // Properties
            builder.Property(sc => sc.SubCategoryName)
                   .IsRequired()
                   .HasMaxLength(100);

            // 🔥 Relationship: SubCategory → Category (Many-to-One)
            builder.HasOne(sc => sc.Category)
                   .WithMany(c => c.SubCategories)
                   .HasForeignKey(sc => sc.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 🔥 Relationship: SubCategory → Inventory (1:N)
            builder.HasMany(sc => sc.Inventory)
                   .WithOne(i => i.SubCategory)
                   .HasForeignKey(i => i.SubCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}