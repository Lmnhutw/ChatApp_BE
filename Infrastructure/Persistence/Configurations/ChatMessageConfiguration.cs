using ChatApp_BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp_BE.Infrastructure.Persistence.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.SenderId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(message => message.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(message => message.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(message => message.EditedByUserId)
            .HasMaxLength(450);

        builder.Property(message => message.DeletedByUserId)
            .HasMaxLength(450);

        builder.Property(message => message.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(message => message.RowVersion)
            .IsRowVersion();

        builder.HasOne(message => message.Conversation)
            .WithMany(conversation => conversation.Messages)
            .HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(message => message.Sender)
            .WithMany()
            .HasForeignKey(message => message.SenderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(message => message.ReplyToMessage)
            .WithMany(message => message.Replies)
            .HasForeignKey(message => message.ReplyToMessageId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(message => new { message.ConversationId, message.CreatedAt, message.Id });
        builder.HasIndex(message => new { message.SenderId, message.CreatedAt });
        builder.HasIndex(message => message.ReplyToMessageId);
        builder.HasIndex(message => message.DeletedAt);
    }
}
