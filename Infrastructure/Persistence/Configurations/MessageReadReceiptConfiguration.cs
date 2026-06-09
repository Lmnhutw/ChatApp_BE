using ChatApp_BE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp_BE.Infrastructure.Persistence.Configurations;

public class MessageReadReceiptConfiguration : IEntityTypeConfiguration<MessageReadReceipt>
{
    public void Configure(EntityTypeBuilder<MessageReadReceipt> builder)
    {
        builder.ToTable("MessageReadReceipts");

        builder.HasKey(receipt => new { receipt.MessageId, receipt.UserId });

        builder.Property(receipt => receipt.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(receipt => receipt.ReadAt)
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.HasOne(receipt => receipt.Message)
            .WithMany(message => message.ReadReceipts)
            .HasForeignKey(receipt => receipt.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(receipt => receipt.User)
            .WithMany()
            .HasForeignKey(receipt => receipt.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(receipt => new { receipt.UserId, receipt.ReadAt });
    }
}
