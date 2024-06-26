public class MessageViewModel
{
    public string? Content { get; set; }
    public string? FullName { get; set; }
    public int RoomId { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
}