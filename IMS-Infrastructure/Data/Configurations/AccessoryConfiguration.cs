using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class AccessoryConfiguration : IEntityTypeConfiguration<Accessory>
    {
        public void Configure(EntityTypeBuilder<Accessory> builder)
        {
            builder.ToTable("Accessories");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AccessoryName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.PurchaseDate)
                .IsRequired();

            builder.Property(a => a.Brand)
                .HasMaxLength(255);

            builder.Property(a => a.Model)
                .HasMaxLength(255);

            builder.Property(a => a.SerialNumber)
                .HasMaxLength(255);

            builder.HasOne(a => a.Inventory)
                .WithMany(i => i.Accessories)
                .HasForeignKey(a => a.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Category)
                .WithMany()
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.SubCategory)
                .WithMany()
                .HasForeignKey(a => a.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}