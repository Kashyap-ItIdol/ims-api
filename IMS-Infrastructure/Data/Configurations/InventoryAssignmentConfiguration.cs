using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Configurations
{
    public class InventoryAssignmentConfiguration : IEntityTypeConfiguration<InventoryAssignment>
    {
        public void Configure(EntityTypeBuilder<InventoryAssignment> builder)
        {
            // Table
            builder.ToTable("InventoryAssignments");

            // PK
            builder.HasKey(a => a.Id);

            // Properties
            builder.Property(a => a.Location)
                   .HasMaxLength(150);

            builder.Property(a => a.DeskNumber)
                   .HasMaxLength(50);

            builder.Property(a => a.AssignedDate)
                   .IsRequired();

            // 🔥 Relationship: Assignment → Inventory
            builder.HasOne(a => a.Inventory)
                   .WithMany(i => i.Assignments)
                   .HasForeignKey(a => a.InventoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            // 🔥 Relationship: Assignment → User (optional)
            builder.HasOne(a => a.User)
                   .WithMany(u => u.InventoryAssignments)
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}