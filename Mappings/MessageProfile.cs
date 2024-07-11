using AutoMapper;

namespace ChatApp_BE.Mappings
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            //CreateMap<Message, MessageViewModel>()
            //  .ForMember(dst => dst.FromUserName, opt => opt.MapFrom(x => x.ApplicationUser.Nickname))
            //  .ForMember(dst => dst.FromFullName, opt => opt.MapFrom(x => x.ApplicationUser.FullName))
            //   .ForMember(dst => dst.Room, opt => opt.MapFrom(x => x.Room.Name))
            //   .ForMember(dst => dst.Content, opt => opt.MapFrom(x => x.Content));

            //CreateMap<MessageViewModel, Message>();
        }
    }
}