using AutoMapper;
using ChatApp_BE.Models;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<Room, RoomViewModel>()
            .ForMember(dst => dst.RoomName, opt => opt.MapFrom(x => x.Name))
            .ForMember(dst => dst.AdminName, opt => opt.MapFrom(x => x.Admin.FullName));

        CreateMap<RoomUser, RoleUserViewModel>()
            .ForMember(dst => dst.RoomId, opt => opt.MapFrom(x => x.RoomId))
            .ForMember(dst => dst.Email, opt => opt.MapFrom(x => x.User.Email))
            .ForMember(dst => dst.FullName, opt => opt.MapFrom(x => x.User.FullName))
            .ForMember(dst => dst.Role, opt => opt.MapFrom(x => x.IsModerator ? "Moderator" : "Member"));

        CreateMap<RoomViewModel, Room>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(x => x.RoomName));
    }
}