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
        CreateMap<Tour, TourDto>();
        CreateMap<TransportTime, TransportTimeDto>();
        CreateMap<TourPreference, TourPreferenceDto>();
        CreateMap<RecommendedGroupSize, RecommendedGroupSizeDto>();
        CreateMap<TourLength, TourLengthDto>();
    }
}
