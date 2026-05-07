using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class TicketCommentLikeConfiguration : IEntityTypeConfiguration<TicketCommentLike>
    {
        public void Configure(EntityTypeBuilder<TicketCommentLike> builder)
        {
            builder.HasIndex(x => x.CommentId);
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.IsDeleted);
            builder.HasIndex(x => new { x.CommentId, x.UserId, x.IsDeleted }).IsUnique();
            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasOne(x => x.Comment)
                   .WithMany(c => c.Likes)
                   .HasForeignKey(x => x.CommentId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
