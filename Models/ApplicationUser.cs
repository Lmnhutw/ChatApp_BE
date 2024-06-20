using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.Models;

public class ApplicationUser : IdentityUser
{
    public string? Nickname { get; set; }

    public override string? Email { get; set; } = string.Empty;

    public override bool EmailConfirmed { get; set; }

    public override string? PasswordHash { get; set; }

    public override string? SecurityStamp { get; set; }

    public string? FullName { get; set; }

    public string? Avatar { get; set; }

    public string? EmailConfirmationToken { get; set; }

    public string? ResetPasswordToken { get; set; }

    public string? RefreshToken { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}