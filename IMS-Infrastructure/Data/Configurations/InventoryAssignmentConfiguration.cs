using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class InventoryAssignmentConfiguration : IEntityTypeConfiguration<InventoryAssignment>
    {
        public void Configure(EntityTypeBuilder<InventoryAssignment> builder)
        {
            builder.ToTable("InventoryAssignments");

            builder.HasKey(ia => ia.Id);

            builder.Property(ia => ia.AssignedDate)
                .IsRequired();

            builder.Property(ia => ia.Location)
                .HasMaxLength(255);

            builder.Property(ia => ia.DeskNumber)
                .HasMaxLength(255);

            builder.HasOne(ia => ia.Inventory)
                .WithMany(i => i.Assignments)
                .HasForeignKey(ia => ia.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ia => ia.User)
                .WithMany(u => u.InventoryAssignments)
                .HasForeignKey(ia => ia.AssignedTo)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}