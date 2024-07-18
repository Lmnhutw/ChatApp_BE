using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    public string? Name { get; set; } = null!;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();
}