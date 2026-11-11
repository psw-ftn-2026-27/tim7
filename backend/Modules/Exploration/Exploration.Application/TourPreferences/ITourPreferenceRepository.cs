using Exploration.Domain.TourPreferences;

namespace Exploration.Application.TourPreferences;

public interface ITourPreferenceRepository
{
    Task<TourPreference?> GetByUserAsync(Guid userId);
    void Add(TourPreference tourPreference);
    void Remove(TourPreference tourPreference);
}
