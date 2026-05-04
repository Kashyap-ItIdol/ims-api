using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.Property(i => i.Model)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(i => i.Brand)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(i => i.SerialNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(i => i.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(i => i.Condition)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(i => i.Location)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(i => i.Table)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(i => i.ItemPictureUrl)
                   .HasMaxLength(500);

           
            builder.HasOne(i => i.Category)
                   .WithMany()
                   .HasForeignKey(i => i.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Subcategory)
                   .WithMany(s => s.Inventory)
                   .HasForeignKey(i => i.SubcategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.PurchaseDetail)
                   .WithOne(p => p.Inventory)
                   .HasForeignKey<PurchaseDetail>(p => p.InventoryId);

            builder.HasIndex(i => i.SerialNumber).IsUnique();
            builder.HasIndex(i => i.CategoryId);
            builder.HasIndex(i => i.SubcategoryId);
        }
    }
}
