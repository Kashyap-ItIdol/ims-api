using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class InventoryAssignmentConfiguration : IEntityTypeConfiguration<InventoryAssignment>
    {
        public void Configure(EntityTypeBuilder<InventoryAssignment> builder)
        {

            builder.Property(a => a.Location)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(a => a.Table)
                   .IsRequired()
                   .HasMaxLength(100);

            // Relationship with Inventory
            builder.HasOne(a => a.Inventory)
                   .WithMany(i => i.InventoryAssignments)
                   .HasForeignKey(a => a.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
