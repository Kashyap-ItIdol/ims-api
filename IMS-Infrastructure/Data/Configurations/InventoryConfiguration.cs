using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("Inventories");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ItemName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(i => i.Brand).HasMaxLength(100);
            builder.Property(i => i.Model).HasMaxLength(100);
            builder.Property(i => i.SerialNumber).HasMaxLength(100);

            builder.Property(i => i.Status).IsRequired();
            builder.Property(i => i.Condition).IsRequired();

            builder.HasOne(i => i.Category)
                   .WithMany(c => c.Inventory)
                   .HasForeignKey(i => i.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.SubCategory)
                   .WithMany(sc => sc.Inventory)
                   .HasForeignKey(i => i.SubCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}