using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class TicketStatusHistoryConfiguration : IEntityTypeConfiguration<TicketStatusHistory>
    {
        public void Configure(EntityTypeBuilder<TicketStatusHistory> builder)
        {
            // Indexes
            builder.HasIndex(x => x.TicketId);
            builder.HasIndex(x => x.ChangedBy);
            builder.HasIndex(x => x.ChangedAt);

            // Validations
            builder.Property(x => x.OldStatusId).IsRequired();
            builder.Property(x => x.NewStatusId).IsRequired();

            // Relationships
            builder.HasOne<Ticket>()
                   .WithMany()
                   .HasForeignKey(x => x.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.ChangedBy)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
