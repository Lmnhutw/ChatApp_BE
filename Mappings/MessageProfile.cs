using AutoMapper;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels;

namespace ChatApp_BE.Mappings
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            //CreateMap<Message, MessageViewModel>()
            //  .ForMember(dst => dst.FromUserName, opt => opt.MapFrom(x => x.User.Nickname))
            //  .ForMember(dst => dst.FromFullName, opt => opt.MapFrom(x => x.User.FullName))
            //   .ForMember(dst => dst.Room, opt => opt.MapFrom(x => x.Room.Name))
            //   .ForMember(dst => dst.Content, opt => opt.MapFrom(x => x.Content));

            //CreateMap<MessageViewModel, Message>();
        }
    }
}