using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Configurations
{
    public class AccessoryConfiguration : IEntityTypeConfiguration<Accessory>
    {
        public void Configure(EntityTypeBuilder<Accessory> builder)
        {
            // Table
            builder.ToTable("Accessories");

            // PK
            builder.HasKey(a => a.Id);

            // Properties
            builder.Property(a => a.AccessoryName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(a => a.Brand)
                   .HasMaxLength(100);

            builder.Property(a => a.Model)
                   .HasMaxLength(100);

            builder.Property(a => a.SerialNumber)
                   .HasMaxLength(100);

            builder.Property(a => a.Condition)
                   .IsRequired();

            builder.Property(a => a.PurchaseDate)
                   .IsRequired();

            // 🔥 Relationship: Accessory → Inventory (Many-to-One)
            builder.HasOne(a => a.Inventory)
                   .WithMany(i => i.Accessories)
                   .HasForeignKey(a => a.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Category)
        .WithMany()
        .HasForeignKey(a => a.CategoryId)
        .OnDelete(DeleteBehavior.NoAction); // 🔥

            builder.HasOne(a => a.SubCategory)
                   .WithMany()
                   .HasForeignKey(a => a.SubCategoryId)
                   .OnDelete(DeleteBehavior.NoAction); // 🔥
        }
    }
}