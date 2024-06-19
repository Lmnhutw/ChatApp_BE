using ChatApp_BE.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Collections.Generic;

using ChatApp_BE.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Reflection;
using System.Reflection.Emit;

namespace ChatApp_BE.Data;

public partial class ChatAppContext : IdentityDbContext<User>
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
    }
}