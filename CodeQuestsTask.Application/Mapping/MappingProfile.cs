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
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            // mapping from Match to MatchPublisherDto
            CreateMap<MatchPublisherDto, Match>()
                .ReverseMap()
                .ForMember(opts => opts.PublisherName, x => x.MapFrom(u => u.User!.Name));

            CreateMap<ApplicationUser, RegisterDto>().ReverseMap();
        }
    }
}
