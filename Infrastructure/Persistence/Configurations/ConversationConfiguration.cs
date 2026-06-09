using ChatApp_BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp_BE.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(conversation => conversation.Id);

        builder.Property(conversation => conversation.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(conversation => conversation.Title)
            .HasMaxLength(200);

        builder.Property(conversation => conversation.CreatedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(conversation => conversation.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(conversation => conversation.RowVersion)
            .IsRowVersion();

        builder.HasOne(conversation => conversation.CreatedByUser)
            .WithMany()
            .HasForeignKey(conversation => conversation.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(conversation => new { conversation.Type, conversation.CreatedAt });
        builder.HasIndex(conversation => conversation.CreatedByUserId);
        builder.HasIndex(conversation => conversation.DeletedAt);
    }
}
