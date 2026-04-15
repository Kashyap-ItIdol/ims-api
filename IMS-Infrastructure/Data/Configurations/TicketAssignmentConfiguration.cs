using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class TicketAssignmentConfiguration : IEntityTypeConfiguration<TicketAssignment>
    {
        public void Configure(EntityTypeBuilder<TicketAssignment> builder)
        {

            builder.HasIndex(x => x.TicketId);
            builder.HasIndex(x => x.assignedTo);
            builder.HasIndex(x => x.assigned_by);
            builder.HasIndex(x => x.assigned_at);

            builder.Property(x => x.status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasOne<Ticket>()
                   .WithMany(t => t.TicketAssignments)
                   .HasForeignKey(x => x.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.assignedTo)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.assigned_by)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
