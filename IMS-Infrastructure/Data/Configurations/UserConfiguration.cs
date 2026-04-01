using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Indexes
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.RoleId);
            builder.HasIndex(x => x.DepartmentId);
            builder.HasIndex(x => new { x.IsDeleted, x.IsActive });
            builder.HasIndex(x => x.CreatedAt);

            // validations
            builder.Property(x => x.FullName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(x => x.PasswordHash)
                   .IsRequired();

            // Relationships
            builder.HasOne(x => x.Role)
                   .WithMany()
                   .HasForeignKey(x => x.RoleId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                   .WithMany()
                   .HasForeignKey(x => x.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Soft Delete
            builder.HasQueryFilter(x => !x.IsDeleted);

        }
    }
}
