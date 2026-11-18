using Exploration.Application.TourPreferences;
using Exploration.Domain.TourPreferences;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourPreferences;

internal sealed class TourPreferenceRepository : ITourPreferenceRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourPreferenceRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourPreference?> GetByUserAsync(Guid userId)
    {
        return _dbContext.TourPreferences.FirstOrDefaultAsync(tourPreference => tourPreference.UserId == userId);
    }

    public void Add(TourPreference tourPreference)
    {
        _dbContext.TourPreferences.Add(tourPreference);
    }

    public void Remove(TourPreference tourPreference)
    {
        _dbContext.TourPreferences.Remove(tourPreference);
    }
}
