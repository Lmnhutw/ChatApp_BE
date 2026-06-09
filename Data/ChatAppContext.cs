using ChatApp_BE.Domain.Entities;
using ChatApp_BE.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatApp_BE.Data;

public partial class ChatAppContext : IdentityDbContext<ApplicationUser>
{
    public ChatAppContext(DbContextOptions<ChatAppContext> options)
          : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }

    public DbSet<Room> Rooms { get; set; }

    public DbSet<RoomUser> RoomUsers { get; set; }

    public DbSet<Conversation> Conversations { get; set; }

    public DbSet<ConversationMember> ConversationMembers { get; set; }

    public DbSet<ChatMessage> ChatMessages { get; set; }

    public DbSet<MessageAttachment> MessageAttachments { get; set; }

    public DbSet<MessageReaction> MessageReactions { get; set; }

    public DbSet<MessageReadReceipt> MessageReadReceipts { get; set; }

    public DbSet<UserBlock> UserBlocks { get; set; }

    public DbSet<UserPresence> UserPresences { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ChatAppContext).Assembly);

        builder.Entity<RoomUser>().HasKey(ru => new { ru.RoomId, ru.Id });
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName is not null && tableName.StartsWith("AspNet"))
            {
                entityType.SetTableName(tableName.Substring(6));
            }
        }
    }
}
