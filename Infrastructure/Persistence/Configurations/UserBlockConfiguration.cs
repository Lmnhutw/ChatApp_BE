using ChatApp_BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp_BE.Infrastructure.Persistence.Configurations;

public class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.ToTable("UserBlocks");

        builder.HasKey(block => new { block.BlockerUserId, block.BlockedUserId });

        builder.Property(block => block.BlockerUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(block => block.BlockedUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(block => block.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(block => block.BlockerUser)
            .WithMany()
            .HasForeignKey(block => block.BlockerUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(block => block.BlockedUser)
            .WithMany()
            .HasForeignKey(block => block.BlockedUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(block => new { block.BlockedUserId, block.CreatedAt });
    }
}
