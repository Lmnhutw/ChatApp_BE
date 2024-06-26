using ChatApp_BE.Models;

public class RoomViewModel
{
    public int RoomId { get; set; }
    public string? RoomName { get; set; }
    public string? CreatedBy { get; set; }
    public string? AdminName { get; set; }
    public List<RoleUserViewModel>? Members { get; set; }
}