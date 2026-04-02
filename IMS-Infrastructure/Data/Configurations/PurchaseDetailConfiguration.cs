using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class PurchaseDetailConfiguration : IEntityTypeConfiguration<PurchaseDetail>
    {
        public void Configure(EntityTypeBuilder<PurchaseDetail> builder)
        {

            builder.Property(p => p.Vendor)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.InvoiceNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.PurchaseCost)
                   .HasPrecision(18, 2);

            builder.Property(p => p.PurchaseDate)
                   .IsRequired();

            builder.Property(p => p.AmcExpiry)
                   .IsRequired();

            builder.HasOne(p => p.Inventory)
                   .WithOne(i => i.PurchaseDetail)
                   .HasForeignKey<PurchaseDetail>(p => p.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
