using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Configurations
{
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.SerialNo)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(x => x.SerialNo)
                .IsUnique();

           
            builder.HasOne(x => x.Category)
                .WithMany(c => c.Assets)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SubCategory)
                .WithMany(sc => sc.Assets)
                .HasForeignKey(x => x.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssetStatus)
                .WithMany(s => s.Assets)
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssetCondition)
                .WithMany(c => c.Assets)
                .HasForeignKey(x => x.ConditionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.AssignedUser)
       .WithMany()
       .HasForeignKey(a => a.AssignedTo)
       .OnDelete(DeleteBehavior.Restrict);
        }
    }
}