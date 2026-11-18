using Exploration.Application.TourPreferenceSetup;

namespace Exploration.Application.TourPreferences;

public interface ITourPreferenceReadRepository
{
    Task<TourPreferenceDto?> GetByUserAsync(Guid userId);
}
