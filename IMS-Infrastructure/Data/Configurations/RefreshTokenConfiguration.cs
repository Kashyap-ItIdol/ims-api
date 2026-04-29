using IMS_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IMS_Infrastructure.Data.Configurations
{
    public  class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.Property(x => x.Token)
                   .IsRequired();

            

            builder.HasIndex(x => x.Token).IsUnique();
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Expires);

            builder.Property(x => x.Expires)
                   .IsRequired();

            builder.HasOne(x => x.User)
                   .WithMany(u => u.RefreshTokens)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
