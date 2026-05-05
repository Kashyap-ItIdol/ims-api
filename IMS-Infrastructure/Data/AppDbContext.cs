using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMS_Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }

        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketAssignment> TicketAssignments { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketCommentLike> TicketCommentLikes { get; set; }
        public DbSet<TicketCommentReaction> TicketCommentReactions { get; set; }
        public DbSet<TicketStatusHistory> TicketStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
