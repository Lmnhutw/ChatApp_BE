using ChatApp_BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp_BE.Infrastructure.Persistence.Configurations;

public class UserPresenceConfiguration : IEntityTypeConfiguration<UserPresence>
{
    public void Configure(EntityTypeBuilder<UserPresence> builder)
    {
        builder.ToTable("UserPresences");

        builder.HasKey(presence => presence.UserId);

        builder.Property(presence => presence.UserId)
            .HasMaxLength(450);

        builder.Property(presence => presence.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(presence => presence.LastSeenAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(presence => presence.UpdatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(presence => presence.User)
            .WithOne()
            .HasForeignKey<UserPresence>(presence => presence.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(presence => new { presence.Status, presence.LastSeenAt });
    }
}
