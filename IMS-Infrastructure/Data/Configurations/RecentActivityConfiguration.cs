using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class RecentActivityConfiguration : IEntityTypeConfiguration<RecentActivity>
    {
        public void Configure(EntityTypeBuilder<RecentActivity> builder)
        {
            builder.ToTable("RecentActivities");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemName)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Action)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Details)
                   .IsRequired();

            builder.Property(x => x.DateTime)
                   .IsRequired();

            builder.Property(x => x.IsDeleted)
                   .IsRequired();

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.DateTime);

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

