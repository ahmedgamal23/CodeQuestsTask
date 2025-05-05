using AutoMapper;
using CodeQuestsTask.Domain.Models;
using CodeQuestsTask.Domain.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // mapping from Match to MatchPublisherDto  
            CreateMap<MatchPublisherDto, Match>()
                .ReverseMap()
                .ForMember(opts => opts.PublisherName, x => x.MapFrom(u => u.User!.Name != null ? u.User.Name : null ));

            CreateMap<ApplicationUser, RegisterDto>().ReverseMap();
            CreateMap<UserMatchDto, Playlist>().ReverseMap();
            CreateMap<Playlist, UserMatchViewDto>()
                .ForMember(dest => dest.MatchStatus, opt => opt.MapFrom(src => src.Match!.MatchStatus))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Match!.Description))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Match!.Title))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Match!.CreatedAt))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.Match!.IsDeleted))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Match!.ImageUrl))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.Match!.VideoUrl))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User!.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User!.Email));


        }
    }
}
