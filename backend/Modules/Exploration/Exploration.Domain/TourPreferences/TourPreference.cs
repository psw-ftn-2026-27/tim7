using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourPreferences;

public sealed class TourPreference : AggregateRoot
{
    public Guid UserId { get; private set; }
    public List<TourDifficulty> Difficulties { get; private set; }
    public TransportMode? PreferredTransportMode { get; private set; }
    public List<string> Tags { get; private set; }
    public bool ApplyPreferencesAutomatically { get; private set; }

    private TourPreference()
    {
        Difficulties = null!;
        Tags = null!;
    }

    public TourPreference(Guid userId, List<TourDifficulty> difficulties, TransportMode? preferredTransportMode,
        List<string> tags, bool applyPreferencesAutomatically)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        Difficulties = [];
        Tags = [];
        Update(difficulties, preferredTransportMode, tags, applyPreferencesAutomatically);
    }

    public void Update(List<TourDifficulty> difficulties, TransportMode? preferredTransportMode,
        List<string> tags, bool applyPreferencesAutomatically)
    {
        difficulties ??= [];
        tags ??= [];
        if (difficulties.Count == 0 && preferredTransportMode is null && tags.Count == 0)
        {
            throw new DomainException("Tour preferences require at least one difficulty, a preferred transport mode, or at least one tag.");
        }

        Difficulties = difficulties;
        PreferredTransportMode = preferredTransportMode;
        Tags = tags;
        ApplyPreferencesAutomatically = applyPreferencesAutomatically;
    }
}
