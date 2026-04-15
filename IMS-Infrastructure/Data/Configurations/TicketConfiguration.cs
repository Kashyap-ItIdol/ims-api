using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.HasIndex(x => x.CreatedBy);
            builder.HasIndex(x => x.AssetId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.UpdatedAt);

            builder.Property(x => x.Title)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.Description)
                   .HasMaxLength(5000);

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(x => x.CreatedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Comments)
                   .WithOne()
                   .HasForeignKey(c => c.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.TicketAssignments)
                   .WithOne()
                   .HasForeignKey(a => a.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
