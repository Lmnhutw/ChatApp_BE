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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<RoomUser>().HasKey(ru => new { ru.RoomId, ru.Id });
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName.StartsWith("AspNet"))
            {
                entityType.SetTableName(tableName.Substring(6));
            }
        }
    }
}