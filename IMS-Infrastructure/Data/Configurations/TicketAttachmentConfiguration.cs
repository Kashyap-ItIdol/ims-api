using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
    {
        public void Configure(EntityTypeBuilder<TicketAttachment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FilePath)
                    .IsRequired();

            builder.Property(x => x.TicketId)
                .IsRequired();

            builder.Property(x => x.UserId)
               .IsRequired();

            builder.HasOne(x => x.User)
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Ticket)
                   .WithMany(t => t.Attachments)
                   .HasForeignKey(x => x.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
