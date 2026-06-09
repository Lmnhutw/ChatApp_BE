using ChatApp_BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp_BE.Infrastructure.Persistence.Configurations;

public class MessageReactionConfiguration : IEntityTypeConfiguration<MessageReaction>
{
    public void Configure(EntityTypeBuilder<MessageReaction> builder)
    {
        builder.ToTable("MessageReactions");

        builder.HasKey(reaction => reaction.Id);

        builder.Property(reaction => reaction.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(reaction => reaction.Reaction)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(reaction => reaction.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(reaction => reaction.Message)
            .WithMany(message => message.Reactions)
            .HasForeignKey(reaction => reaction.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(reaction => reaction.User)
            .WithMany()
            .HasForeignKey(reaction => reaction.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(reaction => new { reaction.MessageId, reaction.UserId, reaction.Reaction })
            .IsUnique();

        builder.HasIndex(reaction => new { reaction.UserId, reaction.CreatedAt });
    }
}
