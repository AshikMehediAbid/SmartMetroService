using AutoMapper;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterUserDto, User>().ReverseMap();

        CreateMap<StationCreationDto, Station>()
            .ForMember(dest => dest.Latitude,
                opt => opt.MapFrom(dest => dest.Lat))
            .ForMember(dest => dest.Longitude,
                opt => opt.MapFrom(dest => dest.Long));
    }

}
