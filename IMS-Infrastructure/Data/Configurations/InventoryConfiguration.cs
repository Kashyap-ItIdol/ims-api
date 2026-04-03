using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("Inventories");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ItemName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(i => i.Brand)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(i => i.Model)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(i => i.SerialNumber)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(i => i.Status)
                .IsRequired();

            builder.Property(i => i.Condition)
                .IsRequired();

            builder.HasOne(i => i.Category)
                .WithMany(c => c.Inventory)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.SubCategory)
                .WithMany(sc => sc.Inventory)
                .HasForeignKey(i => i.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.PurchaseDetail)
                .WithMany()
                .HasForeignKey(i => i.PurchaseDetailId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}