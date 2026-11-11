using Exploration.Application.TourPreferences;
using Exploration.Application.Tours;

namespace Exploration.Application.TourPreferenceSetup;

public sealed class TourPreferenceSetupQueries
{
    private readonly ITourPreferenceReadRepository _tourPreferenceReadRepository;
    private readonly ITourReadRepository _tourReadRepository;

    public TourPreferenceSetupQueries(ITourPreferenceReadRepository tourPreferenceReadRepository,
        ITourReadRepository tourReadRepository)
    {
        _tourPreferenceReadRepository = tourPreferenceReadRepository;
        _tourReadRepository = tourReadRepository;
    }

    public async Task<MyTourPreferencesDto> GetMineAsync(Guid userId)
    {
        var preferences = await _tourPreferenceReadRepository.GetByUserAsync(userId);

        return new MyTourPreferencesDto(preferences);
    }

    public Task<List<string>> GetAvailableTagsAsync()
    {
        return _tourReadRepository.GetPublishedTagsAsync();
    }
}
