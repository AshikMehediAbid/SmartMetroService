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


        CreateMap<Station, StationDetailsDto>()
            .ForMember(dest => dest.Lat,
                opt => opt.MapFrom(dest => dest.Latitude))
            .ForMember(dest => dest.Long,
                opt => opt.MapFrom(dest => dest.Longitude));


        CreateMap<Ticket, TicketResponseDto>()
            .ForMember(dest => dest.FromStationName,
                opt => opt.MapFrom(src => src.FromStation == null ? null : src.FromStation.StationName))
            .ForMember(dest => dest.ToStationName,
                opt => opt.MapFrom(src => src.ToStation == null ? null : src.ToStation.StationName))
            .ForMember(dest => dest.ExpiredAt,
                opt => opt.MapFrom(src => src.ExpiryTime))
            .ForMember(dest => dest.QrCode,
                opt => opt.MapFrom(src => src.QRByte == null
                                        ? null
                                        : $"data:image/png;base64,{Convert.ToBase64String(src.QRByte)}")
                );
    }

}
