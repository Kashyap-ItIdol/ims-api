using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Configurations
{
    public class AssetAssignmentConfiguration : IEntityTypeConfiguration<AssetAssignment>
    {
        public void Configure(EntityTypeBuilder<AssetAssignment> entity)
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AssetId)
                .IsRequired();

            entity.Property(x => x.EmployeeId)
                .IsRequired();

            entity.Property(x => x.AssignedDate)
                .IsRequired();

            entity.Property(x => x.OfficeNo)
                .HasMaxLength(20);

            entity.Property(x => x.TableNo)
                .HasMaxLength(20);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(x => x.CreatedBy)
                .IsRequired();

            entity.Property(x => x.IsDeleted)
                .HasDefaultValue(false);

            entity.HasIndex(x => x.AssetId);
            entity.HasIndex(x => x.EmployeeId);
            entity.HasIndex(x => x.AssignedDate);
            entity.HasIndex(x => x.IsDeleted);
            entity.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
