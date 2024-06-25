using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.Models;

public class Message
{
    [Key]
    public int MessageId { get; set; }

    public string? FullName { get; set; }

    public string Content { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public int RoomId { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual ApplicationUser? User { get; set; }
}