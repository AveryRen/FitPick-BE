namespace FitPick_EXE201.Helpers
{
    using AutoMapper;
    using FitPick_EXE201.Models.DTOs;
    using FitPick_EXE201.Models.Entities;
    using FitPick_EXE201.Models.Requests;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Healthprofile, HealthprofileDTO>().ReverseMap();
            CreateMap<HealthprofileRequest, Healthprofile>().ReverseMap();

            CreateMap<Ingredient, IngredientDTO>().ReverseMap();

            CreateMap<Notification, NotificationDTO>()
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type != null ? src.Type.Name : null))
                .ReverseMap();
            CreateMap<NotificationType, NotificationTypeDTO>().ReverseMap();

        }
    }
}
