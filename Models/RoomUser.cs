using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp_BE.Models;

public class RoomUser
{
    public string Id { get; set; } = string.Empty;

    public int RoomId { get; set; }
    public bool IsMember { get; set; }

    [ForeignKey("RoomId")]
    public virtual Room Room { get; set; } = null!;

    [ForeignKey("Id")]
    public virtual ApplicationUser User { get; set; } = null!;
}