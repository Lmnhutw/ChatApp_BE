using ChatApp_BE.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Collections.Generic;

using ChatApp_BE.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ChatApp_BE.Data;

public partial class ChatAppContext : DbContext
{
    public ChatAppContext()
    {
    }

    public ChatAppContext(DbContextOptions<ChatAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomUser> RoomUsers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=MINHNHUT\\MINHNHUT;Initial Catalog=ChatApp;Integrated Security=True;Encrypt=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C0C9C8BC773E0");

            entity.HasIndex(e => e.RoomId, "IX_Messages_RoomId");

            entity.HasIndex(e => e.UserId, "IX_Messages_UserId");

            entity.Property(e => e.Content).HasMaxLength(500);

            entity.HasOne(d => d.Room).WithMany(p => p.Messages).HasForeignKey(d => d.RoomId);

            entity.HasOne(d => d.User).WithMany(p => p.Messages).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__32863939BBE2EA8A");

            entity.HasIndex(e => e.Name, "IX_Rooms_Name").IsUnique();

            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Admin).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_Rooms_Users");
        });

        modelBuilder.Entity<RoomUser>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoomId });

            entity.Property(e => e.IsMember).HasDefaultValue(true);

            entity.HasOne(d => d.Room).WithMany(p => p.RoomUsers)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_RoomUsers_Rooms");

            entity.HasOne(d => d.User).WithMany(p => p.RoomUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RoomUsers_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C480EA3CC");

            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Nickname).HasMaxLength(256);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    //working on
    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

public partial class ChatAppContext
{
    private partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        throw new NotImplementedException();
    }
}