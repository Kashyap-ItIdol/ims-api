using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class ClientAssetAttachmentConfiguration : IEntityTypeConfiguration<ClientAssetAttachment>
    {
        public void Configure(EntityTypeBuilder<ClientAssetAttachment> builder)
        {
            // Table name
            builder.ToTable("ClientAssetAttachments");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.FileName)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(x => x.FilePath)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.UploadedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(x => x.ClientAssetId);

            builder.HasIndex(x => x.UploadedAt);

            // Relationships
            builder.HasOne(caa => caa.ClientAsset)
                   .WithMany(ca => ca.Attachments)
                   .HasForeignKey(caa => caa.ClientAssetId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Audit relationships
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.DeletedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            // Soft delete filter
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
