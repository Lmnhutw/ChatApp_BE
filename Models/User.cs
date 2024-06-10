using System;
using System.Collections.Generic;

namespace ChatApp_BE.Models;

public partial class User
{
    public int UserId { get; set; }

    public string? Nickname { get; set; }

    public string Email { get; set; } = null!;

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? FullName { get; set; }

    public string? Avatar { get; set; }

    public string? EmailConfirmationToken { get; set; }

    public string? ResetPasswordToken { get; set; }

    public string? RefreshToken { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
