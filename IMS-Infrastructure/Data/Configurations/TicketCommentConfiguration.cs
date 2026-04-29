using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
    {
        public void Configure(EntityTypeBuilder<TicketComment> builder)
        {
            builder.HasIndex(x => x.TicketId);
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.CreatedAt);

            builder.Property(x => x.CommentText)
                   .IsRequired()
                   .HasMaxLength(4000);

            builder.HasOne<Ticket>()
                   .WithMany(t => t.Comments)
                   .HasForeignKey(x => x.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
