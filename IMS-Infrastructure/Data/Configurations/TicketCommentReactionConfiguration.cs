using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class TicketCommentReactionConfiguration : IEntityTypeConfiguration<TicketCommentReaction>
    {
        public void Configure(EntityTypeBuilder<TicketCommentReaction> builder)
        {
            builder.HasIndex(x => x.CommentId);
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.IsDeleted);
            builder.HasIndex(x => new { x.CommentId, x.UserId, x.IsDeleted }).IsUnique();
            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.Property(x => x.ReactionType)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasOne(x => x.Comment)
                   .WithMany(c => c.Reactions)
                   .HasForeignKey(x => x.CommentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
