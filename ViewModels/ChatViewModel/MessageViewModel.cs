using ChatApp_BE.Models;

public class MessageViewModel
{
    public int MessageId { get; set; }
    public string UserId { get; set; }
    public string RoomName { get; set; }
    public string? Content { get; set; }
    public string? FullName { get; set; }
    public int RoomId { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
}