using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Configurations
{
    public class ClientAssetAssignmentConfiguration : IEntityTypeConfiguration<ClientAsset>
    {
        public void Configure(EntityTypeBuilder<ClientAsset> entity)
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssetName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(x => x.Status)
                .HasMaxLength(50);

            entity.Property(x => x.Brand)
                .HasMaxLength(100);

            entity.Property(x => x.Model)
                .HasMaxLength(100);

            entity.Property(x => x.SerialNumber)
                .HasMaxLength(100);

            entity.Property(x => x.Condition)
                .HasMaxLength(50);

            entity.Property(x => x.ItemPhoto)
                .HasMaxLength(500);

            entity.Property(x => x.ClientName)
                .HasMaxLength(255);

            entity.Property(x => x.ClientPOC)
                .HasMaxLength(100);

            entity.Property(x => x.SalesPOC)
                .HasMaxLength(100);

            entity.Property(x => x.Location)
                .HasMaxLength(100);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(x => x.IsDeleted)
                .HasDefaultValue(false);
        }
    }
}