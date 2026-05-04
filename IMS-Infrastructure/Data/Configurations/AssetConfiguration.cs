using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            // Table name
            builder.ToTable("Assets");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.AssetName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(x => x.Brand)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Model)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.SerialNumber)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Condition)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.ClientPOC)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.SalesPOC)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(x => x.SerialNumber)
                   .IsUnique();

            builder.HasIndex(x => new { x.CategoryId, x.SubCategoryId });

            // Relationships
            builder.HasOne(a => a.Category)
                   .WithMany(c => c.Assets)
                   .HasForeignKey(a => a.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.SubCategory)
                   .WithMany(sc => sc.Assets)
                   .HasForeignKey(a => a.SubCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Audit relationships
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.CreatedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.UpdatedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.DeletedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            // Soft delete filter
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
