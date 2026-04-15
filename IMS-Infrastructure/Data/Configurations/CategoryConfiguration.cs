using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => x.Name)
                   .IsUnique();

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.CreatedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.UpdatedAt);

            builder.Property(x => x.UpdatedBy);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.UpdatedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.DeletedAt);

            builder.Property(x => x.DeletedBy);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.DeletedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.SubCategories)
                   .WithOne(s => s.Category)
                   .HasForeignKey(s => s.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
