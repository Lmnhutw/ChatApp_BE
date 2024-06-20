using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ChatApp_BE.Models;

public class Room
{
    [Key]
    public int RoomId { get; set; }

    public string Name { get; set; } = null!;

    public int AdminId { get; set; }

    public virtual ApplicationUser Admin { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();
}