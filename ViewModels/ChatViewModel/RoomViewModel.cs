public class RoomViewModel
{
    public int RoomId { get; set; }
    public string? RoomName { get; set; }
    public string? CreatedBy { get; set; }
    public string? UserId { get; set; } // Add UserId field
    public List<RoleUserViewModel>? Members { get; set; }
}