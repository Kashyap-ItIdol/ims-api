using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class AssignedAssetConfiguration : IEntityTypeConfiguration<AssignedAsset>
    {
        public void Configure(EntityTypeBuilder<AssignedAsset> builder)
        {
            builder.ToTable("AssignedAssets");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.ClientAssetId)
                .IsRequired(false);

            builder.HasOne(x => x.ClientAsset)
                .WithMany()
                .HasForeignKey(x => x.ClientAssetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.SerialNumber)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.AssignedDate)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.CreatedBy);
            builder.Property(x => x.UpdatedAt);
            builder.Property(x => x.UpdatedBy);
            builder.Property(x => x.DeletedAt);
            builder.Property(x => x.DeletedBy);

            builder.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}