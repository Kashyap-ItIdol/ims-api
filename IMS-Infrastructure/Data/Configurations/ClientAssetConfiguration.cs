using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class ClientAssetConfiguration : IEntityTypeConfiguration<ClientAsset>
    {
        public void Configure(EntityTypeBuilder<ClientAsset> builder)
        {
            // Table name
            builder.ToTable("ClientAssets");

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

            builder.Property(x => x.ClientName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.ClientPOC)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.SalesPOC)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Location)
                   .HasMaxLength(200);

            builder.Property(x => x.ItemPhoto)
                   .HasMaxLength(500);

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(x => x.SerialNumber)
                   .IsUnique();

            builder.HasIndex(x => new { x.CategoryId, x.SubCategoryId });

            builder.HasIndex(x => x.AssignedTo);

            builder.HasIndex(x => x.CreatedAt);

            // Relationships
            builder.HasOne(ca => ca.Category)
                   .WithMany()
                   .HasForeignKey(ca => ca.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ca => ca.SubCategory)
                   .WithMany()
                   .HasForeignKey(ca => ca.SubCategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ca => ca.AssignedUser)
                   .WithMany()
                   .HasForeignKey(ca => ca.AssignedTo)
                   .OnDelete(DeleteBehavior.SetNull);

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
