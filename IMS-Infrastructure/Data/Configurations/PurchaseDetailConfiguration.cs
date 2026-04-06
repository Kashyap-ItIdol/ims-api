using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Configurations
{
    public class PurchaseDetailConfiguration : IEntityTypeConfiguration<PurchaseDetail>
    {
        public void Configure(EntityTypeBuilder<PurchaseDetail> builder)
        {
            // Table Name
            builder.ToTable("PurchaseDetails");

            // Primary Key
            builder.HasKey(pd => pd.Id);

            // Properties
            builder.Property(pd => pd.Vendor)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(pd => pd.InvoiceNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(pd => pd.PurchaseCost)
                   .HasColumnType("decimal(18,2)");

            builder.Property(pd => pd.WarrantyExpiry)
                   .IsRequired();

            builder.Property(pd => pd.AmcExpiry)
                   .IsRequired();

            // 🔥 One-to-One Relationship (with Inventory)
            builder.HasOne(pd => pd.Inventory)
                   .WithOne(i => i.PurchaseDetail)
                   .HasForeignKey<PurchaseDetail>(pd => pd.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 🔥 UNIQUE constraint (VERY IMPORTANT for 1:1)
            builder.HasIndex(pd => pd.InventoryId)
                   .IsUnique();
        }
    }
}