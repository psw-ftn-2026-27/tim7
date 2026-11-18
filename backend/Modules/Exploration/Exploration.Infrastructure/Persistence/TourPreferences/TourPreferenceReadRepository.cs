using Exploration.Application.TourPreferences;
using Exploration.Application.TourPreferenceSetup;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourPreferences;

internal sealed class TourPreferenceReadRepository : ITourPreferenceReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourPreferenceReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourPreferenceDto?> GetByUserAsync(Guid userId)
    {
        return _dbContext.TourPreferences
            .Where(tourPreference => tourPreference.UserId == userId)
            .Select(tourPreference => new TourPreferenceDto(
                tourPreference.Difficulties,
                tourPreference.PreferredTransportMode,
                tourPreference.Tags,
                tourPreference.ApplyPreferencesAutomatically))
            .FirstOrDefaultAsync();
    }
}
