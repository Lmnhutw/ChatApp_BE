using AutoMapper;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels;

namespace ChatApp_BE.Mappings
{
    public class RoomProfile : Profile
    {
        public RoomProfile()
        {
            // CreateMap<Room, RoomViewModel>().ForMember(dst => dst.AdminName, opt => opt.MapFrom(x => x.Admin.FullName));

            // CreateMap<RoomViewModel, Room>();
        }
    }
}