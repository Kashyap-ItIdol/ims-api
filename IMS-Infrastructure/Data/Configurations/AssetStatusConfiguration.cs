using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class AssetStatusConfiguration : IEntityTypeConfiguration<AssetStatus>
    {
        public void Configure(EntityTypeBuilder<AssetStatus> builder)
        {
            builder.ToTable("AssetStatuses");

            builder.Property(x => x.Status)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasData(
                new AssetStatus { Id = 1, Status = "Available" },
                new AssetStatus { Id = 2, Status = "Assigned" },
                new AssetStatus { Id = 3, Status = "Under Repair" },
                new AssetStatus { Id = 4, Status = "Scrap" }
            );
        }
    }
}

