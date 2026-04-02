using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            // Properties
            builder.Property(i => i.InventoryName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(i => i.Model)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(i => i.SerialNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(i => i.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            // Relationships

            // Category (Many-to-One)
            builder.HasOne(i => i.Category)
                   .WithMany(c => c.Inventory)
                   .HasForeignKey(i => i.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // SubCategory (Many-to-One)
            builder.HasOne(i => i.Subcategory)
                   .WithMany(s => s.Inventory)
                   .HasForeignKey(i => i.SubcategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

            // One-to-One with PurchaseDetail
            builder.HasOne(i => i.PurchaseDetail)
                   .WithOne(p => p.Inventory)
                   .HasForeignKey<PurchaseDetail>(p => p.InventoryId);

            // Indexes (optional but recommended)
            builder.HasIndex(i => i.SerialNumber).IsUnique();
            builder.HasIndex(i => i.CategoryId);
            builder.HasIndex(i => i.SubcategoryId);
        }
    }
}
