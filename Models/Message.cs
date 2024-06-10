using System;
using System.Collections.Generic;

namespace ChatApp_BE.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime Timestamp { get; set; }

    public int? UserId { get; set; }

    public int RoomId { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual User? User { get; set; }
}
