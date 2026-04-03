using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class PurchaseDetailConfiguration : IEntityTypeConfiguration<PurchaseDetail>
    {
        public void Configure(EntityTypeBuilder<PurchaseDetail> builder)
        {
            builder.ToTable("PurchaseDetails");

            builder.HasKey(pd => pd.Id);

            builder.Property(pd => pd.Vendor)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(pd => pd.PurchaseDate)
                .IsRequired();

            builder.Property(pd => pd.WarrantyExpiry)
                .IsRequired();

            builder.Property(pd => pd.PurchaseCost)
                .HasColumnType("decimal(18,2)");

            builder.Property(pd => pd.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(pd => pd.AmcExpiry)
                .IsRequired();

            builder.HasOne(pd => pd.Inventory)
                .WithOne(i => i.PurchaseDetail)
                .HasForeignKey<PurchaseDetail>(pd => pd.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}