using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace IMS_Infrastructure.Data.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.HasKey(t => t.Id);

            // Relationships
            builder.HasMany(t => t.Comments)
                   .WithOne()
                   .HasForeignKey(c => c.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.TicketAssignments)
                   .WithOne()
                   .HasForeignKey(a => a.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.TicketStatusHistories)
                   .WithOne()
                   .HasForeignKey(h => h.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

