using ChatApp_BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp_BE.Infrastructure.Persistence.Configurations;

public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.ToTable("MessageAttachments");

        builder.HasKey(attachment => attachment.Id);

        builder.Property(attachment => attachment.UploadedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(attachment => attachment.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(attachment => attachment.ContentType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(attachment => attachment.StorageKey)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(attachment => attachment.PublicUrl)
            .HasMaxLength(2048);

        builder.Property(attachment => attachment.CreatedAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(attachment => attachment.Message)
            .WithMany(message => message.Attachments)
            .HasForeignKey(attachment => attachment.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(attachment => attachment.UploadedByUser)
            .WithMany()
            .HasForeignKey(attachment => attachment.UploadedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(attachment => new { attachment.MessageId, attachment.CreatedAt });
        builder.HasIndex(attachment => attachment.UploadedByUserId);
        builder.HasIndex(attachment => attachment.StorageKey);
    }
}
