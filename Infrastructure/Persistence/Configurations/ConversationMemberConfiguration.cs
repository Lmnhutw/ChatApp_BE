using ChatApp_BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp_BE.Infrastructure.Persistence.Configurations;

public class ConversationMemberConfiguration : IEntityTypeConfiguration<ConversationMember>
{
    public void Configure(EntityTypeBuilder<ConversationMember> builder)
    {
        builder.ToTable("ConversationMembers");

        builder.HasKey(member => new { member.ConversationId, member.UserId });

        builder.Property(member => member.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(member => member.Role)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(member => member.JoinedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(member => member.RowVersion)
            .IsRowVersion();

        builder.HasOne(member => member.Conversation)
            .WithMany(conversation => conversation.Members)
            .HasForeignKey(member => member.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(member => member.User)
            .WithMany()
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(member => member.LastReadMessage)
            .WithMany()
            .HasForeignKey(member => member.LastReadMessageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(member => new { member.UserId, member.ConversationId });
        builder.HasIndex(member => new { member.ConversationId, member.LeftAt });
        builder.HasIndex(member => member.LastReadMessageId);
    }
}
