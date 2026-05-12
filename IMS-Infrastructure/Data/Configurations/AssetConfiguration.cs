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
            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Brand)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Model)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.SerialNo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(x => x.SerialNo)
                .IsUnique();

            builder.HasIndex(x => new { x.CategoryId, x.SubCategoryId });

            // Relationships
            builder.HasOne(a => a.Category)
                .WithMany(c => c.Assets)
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.SubCategory)
                .WithMany(sc => sc.Assets)
                .HasForeignKey(a => a.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.AssetStatus)
                .WithMany(s => s.Assets)
                .HasForeignKey(a => a.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.AssetCondition)
                .WithMany(c => c.Assets)
                .HasForeignKey(a => a.ConditionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Audit relationships
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.DeletedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.AssignedUser)
                .WithMany()
                .HasForeignKey(a => a.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.ParentAsset)
                .WithMany(a => a.ChildAssets)
                .HasForeignKey(a => a.ParentAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Soft delete filter
            builder.HasQueryFilter(x => !x.IsActive);
        }
    }
}