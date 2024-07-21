using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatApp_BE.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        public string? Name { get; set; } = null!;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Id { get; set; } = string.Empty; // UserId

        [ForeignKey("Id")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
        public virtual ICollection<RoomUser> RoomUsers { get; set; } = new List<RoomUser>();
    }
}