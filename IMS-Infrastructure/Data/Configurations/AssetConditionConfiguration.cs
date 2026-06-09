using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class AssetConditionConfiguration : IEntityTypeConfiguration<AssetCondition>
    {
        public void Configure(EntityTypeBuilder<AssetCondition> builder)
        {
            builder.ToTable("AssetConditions");

            builder.Property(x => x.Condition)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasData(
                new AssetCondition { Id = 1, Condition = "Good" },
                new AssetCondition { Id = 2, Condition = "Fair" },
                new AssetCondition { Id = 3, Condition = "Critical" }
            );
        }
    }
}

