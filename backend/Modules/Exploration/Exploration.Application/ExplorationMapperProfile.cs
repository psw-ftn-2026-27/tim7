using AutoMapper;
using Exploration.Application.TourPreferenceSetup;
using Exploration.Application.Tours;
using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;

namespace Exploration.Application;

public sealed class ExplorationMapperProfile : Profile
{
    public ExplorationMapperProfile()
    {
        CreateMap<Tour, TourDto>()
            .ForCtorParam(nameof(TourDto.IsFree), options => options.MapFrom(tour => tour.Price == null ? (bool?)null : tour.Price.IsFree))
            .ForCtorParam(nameof(TourDto.Amount), options => options.MapFrom(tour => tour.Price == null ? null : tour.Price.Amount));
        CreateMap<TransportTime, TransportTimeDto>();
        CreateMap<TourPreference, TourPreferenceDto>();
        CreateMap<RecommendedGroupSize, RecommendedGroupSizeDto>();
        CreateMap<TourLength, TourLengthDto>();
    }
}
