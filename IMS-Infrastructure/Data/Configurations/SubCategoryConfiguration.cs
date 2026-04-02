using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class SubCategoryConfiguration : IEntityTypeConfiguration<SubCategory>
    {
        public void Configure(EntityTypeBuilder<SubCategory> builder)
        {

            builder.Property(s => s.Name)
                   .IsRequired()
                   .HasMaxLength(150);

            // Relationship with Category
            builder.HasOne(s => s.Category)
                   .WithMany(c => c.SubCategory)
                   .HasForeignKey(s => s.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
