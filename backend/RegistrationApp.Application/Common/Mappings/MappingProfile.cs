using AutoMapper;
using RegistrationApp.Application.Lookups.Queries;
using RegistrationApp.Application.Registrations.Queries.GetRegistration;
using RegistrationApp.Domain.Entities;

namespace RegistrationApp.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Governorate, LookupDto>();
        CreateMap<City, LookupDto>();

        CreateMap<Registration, RegistrationDto>();
        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.GovernorateNameAr, opt => opt.Ignore())
            .ForMember(dest => dest.GovernorateNameEn, opt => opt.Ignore())
            .ForMember(dest => dest.CityNameAr, opt => opt.Ignore())
            .ForMember(dest => dest.CityNameEn, opt => opt.Ignore());
    }
}
