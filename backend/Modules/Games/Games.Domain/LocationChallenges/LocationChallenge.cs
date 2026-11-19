using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallenge : AggregateRoot
{
    private const double MaxDistanceFromStartMeters = 400;
    private const double MinDistanceBetweenLocationsMeters = 10;
    private const double MaxDistanceToNeighborMeters = 200;
    private const double PointToleranceMeters = 5;

    public string Name { get; private set; }
    public int Xp { get; private set; }
    public Location StartLocation { get; private set; }
    public int RadiusBonusMeters { get; private set; }
    private readonly List<HiddenPoint> _hiddenPoints = [];
    public IReadOnlyList<HiddenPoint> HiddenPoints => _hiddenPoints;

    private LocationChallenge()
    {
        Name = null!;
        StartLocation = null!;
    }

    public LocationChallenge(
        string name,
        int xp,
        Location? startLocation,
        List<HiddenPoint> hiddenPoints,
        int radiusBonusMeters)
        : base(Guid.NewGuid())
    {
        if (name is null || name.Length is < 3 or > 20)
        {
            throw new DomainException("The name of a challenge must have between 3 and 20 characters.");
        }
        if (xp <= 0)
        {
            throw new DomainException("The XP of a challenge must be positive.");
        }
        if (startLocation is null)
        {
            throw new DomainException("A challenge requires a start location.");
        }
        if (hiddenPoints.Count == 0)
        {
            throw new DomainException("A challenge requires at least one hidden point.");
        }
        if (radiusBonusMeters is < 0 or > 100)
        {
            throw new DomainException("The radius bonus must be between 0 and 100 meters.");
        }

        var locations = new List<Location> { startLocation };
        locations.AddRange(hiddenPoints.Select(point => point.Location));
        EnsureLocationsAreValid(startLocation, locations);

        Name = name;
        Xp = xp;
        StartLocation = startLocation;
        RadiusBonusMeters = radiusBonusMeters;
        _hiddenPoints.AddRange(hiddenPoints);
    }

    // The circle reaches the farthest hidden point, plus the bonus drawn once when the challenge was created.
    public double RadiusMeters()
    {
        return HiddenPoints.Max(point => StartLocation.DistanceTo(point.Location)) + RadiusBonusMeters;
    }

    public bool IsInCircle(Location position)
    {
        return StartLocation.DistanceTo(position) <= RadiusMeters();
    }

    public void EnsureInCircle(Location position)
    {
        if (!IsInCircle(position))
        {
            throw new DomainException("The position is outside the circle of the challenge.");
        }
    }

    public HiddenPoint GetUndiscoveredPointNear(Location position, IReadOnlyCollection<Guid> discoveredPointIds)
    {
        EnsureInCircle(position);

        var point = HiddenPoints.FirstOrDefault(candidate =>
            !discoveredPointIds.Contains(candidate.Id)
            && candidate.Location.DistanceTo(position) <= PointToleranceMeters);
        if (point is null)
        {
            throw new DomainException("You are not at the location of an undiscovered point.");
        }

        return point;
    }

    // locations holds the start location first, then the location of every hidden point.
    private static void EnsureLocationsAreValid(Location startLocation, List<Location> locations)
    {
        if (locations.Skip(1).Any(location => startLocation.DistanceTo(location) > MaxDistanceFromStartMeters))
        {
            throw new DomainException("Every hidden point must be at most 400 m from the start location.");
        }

        for (var i = 0; i < locations.Count; i++)
        {
            for (var j = i + 1; j < locations.Count; j++)
            {
                if (locations[i].DistanceTo(locations[j]) < MinDistanceBetweenLocationsMeters)
                {
                    throw new DomainException("Any two locations of a challenge must be at least 10 m apart.");
                }
            }
        }

        for (var i = 0; i < locations.Count; i++)
        {
            var hasNeighbor = locations
                .Where((_, j) => j != i)
                .Any(other => locations[i].DistanceTo(other) < MaxDistanceToNeighborMeters);
            if (!hasNeighbor)
            {
                throw new DomainException("Every location of a challenge must have another location less than 200 m away.");
            }
        }
    }
}
