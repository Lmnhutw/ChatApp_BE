using AutoMapper;
using ChatApp_BE.Models;

public class RoomProfile : Profile
{
    public RoomProfile()
    {
        CreateMap<Room, RoomViewModel>()
            .ForMember(dst => dst.RoomName, opt => opt.MapFrom(x => x.Name));

        CreateMap<RoomUser, RoleUserViewModel>()
            .ForMember(dst => dst.RoomId, opt => opt.MapFrom(x => x.RoomId))
            .ForMember(dst => dst.Email, opt => opt.MapFrom(x => x.User.Email))
            .ForMember(dst => dst.FullName, opt => opt.MapFrom(x => x.User.FullName));

        CreateMap<RoomViewModel, Room>()
            .ForMember(dst => dst.Name, opt => opt.MapFrom(x => x.RoomName));
    }
}