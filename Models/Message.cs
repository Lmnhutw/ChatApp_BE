using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp_BE.Models;

public class Message
{
    [Key]
    public int MessageId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public int RoomId { get; set; }

    public virtual Room Room { get; set; } = null!;
    public string? Id { get; set; } = null!;

    [ForeignKey("Id")]
    public virtual ApplicationUser? User { get; set; }
}