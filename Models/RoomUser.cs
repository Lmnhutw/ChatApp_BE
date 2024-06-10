using System;
using System.Collections.Generic;

namespace ChatApp_BE.Models;

public partial class RoomUser
{
    public int UserId { get; set; }

    public int RoomId { get; set; }

    public bool IsModerator { get; set; }

    public bool IsMember { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
