using System;
using System.Collections.Generic;

namespace ChatApp_BE.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string Name { get; set; } = null!;

    public int AdminId { get; set; }

    public virtual User Admin { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();
}
