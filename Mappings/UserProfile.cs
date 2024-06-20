using AutoMapper;
using ChatApp_BE.Models;
using ChatApp_BE.ViewModels;

namespace ChatApp_BE.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserViewModel>();
            CreateMap<UserViewModel, ApplicationUser>();
        }
    }
}