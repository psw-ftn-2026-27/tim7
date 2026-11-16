using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class Tour : AggregateRoot
{
    private const int MinimumDescriptionLengthForPublishing = 100;
    private const int MinimumKeyPointsForPublishing = 2;

    private readonly List<TransportTime> _transportTimes = [];
    private readonly List<KeyPoint> _keyPoints = [];

    public Guid AuthorId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public TourDifficulty Difficulty { get; private set; }
    public List<string> Tags { get; private set; }
    public TourStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }
    public RecommendedGroupSize RecommendedGroupSize { get; private set; }
    public TourLength? Length { get; private set; }
    public IReadOnlyList<TransportTime> TransportTimes => _transportTimes;
    public IReadOnlyList<KeyPoint> KeyPoints => _keyPoints.OrderBy(keyPoint => keyPoint.Order).ToList();

    private Tour()
    {
        Name = null!;
        Description = null!;
        Tags = null!;
        RecommendedGroupSize = null!;
    }

    public Tour(Guid authorId, string name, string description, TourDifficulty difficulty, List<string> tags, RecommendedGroupSize recommendedGroupSize)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A tour requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A tour requires a description.");
        }
        if (tags is null || tags.Count == 0)
        {
            throw new DomainException("A tour requires at least one tag.");
        }

        AuthorId = authorId;
        Name = name;
        Description = description;
        Difficulty = difficulty;
        Tags = tags;
        RecommendedGroupSize = recommendedGroupSize;
        Status = TourStatus.Draft;
    }

    public void AddTransportTime(TransportMode transport, int minutes)
    {
        if (_transportTimes.Any(time => time.Transport == transport))
        {
            throw new DomainException($"The tour already has a time for {transport}.");
        }

        _transportTimes.Add(new TransportTime(transport, minutes));
    }

    public void AddKeyPoint(string name, string description, string image, Location location, string? secret, bool isPublic)
    {
        EnsureKeyPointsCanChange();

        _keyPoints.Add(new KeyPoint(name, description, image, location, secret, isPublic, _keyPoints.Count + 1));
        RecalculateLength();
    }

    // A missing image keeps the key point's current one.
    public void ChangeKeyPoint(Guid keyPointId, string name, string description, string? image, Location location, string? secret, bool isPublic)
    {
        EnsureKeyPointsCanChange();
        var keyPoint = FindKeyPoint(keyPointId);

        keyPoint.Change(name, description, image ?? keyPoint.Image, location, secret, isPublic);
        RecalculateLength();
    }

    public void RemoveKeyPoint(Guid keyPointId)
    {
        EnsureKeyPointsCanChange();
        var keyPoint = FindKeyPoint(keyPointId);

        _keyPoints.Remove(keyPoint);
        var order = 1;
        foreach (var remaining in KeyPoints)
        {
            remaining.MoveTo(order);
            order++;
        }
        RecalculateLength();
    }

    public void Publish(TourPrice price)
    {
        if (price is null)
        {
            throw new DomainException("A tour can be published only with a price decision.");
        }
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Only a draft tour can be published.");
        }
        EnsurePublishingRulesAreMet();

        Status = TourStatus.Published;
        PublishedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status != TourStatus.Published)
        {
            throw new DomainException("Only a published tour can be archived.");
        }

        Status = TourStatus.Archived;
        ArchivedAt = DateTime.UtcNow;
        PublishedAt = null;
    }

    public void Reactivate()
    {
        if (Status != TourStatus.Archived)
        {
            throw new DomainException("Only an archived tour can be reactivated.");
        }
        EnsurePublishingRulesAreMet();

        Status = TourStatus.Published;
        PublishedAt = DateTime.UtcNow;
        ArchivedAt = null;
    }

    private void EnsurePublishingRulesAreMet()
    {
        if (Description.Length < MinimumDescriptionLengthForPublishing)
        {
            throw new DomainException($"A tour can be published only with a description of at least {MinimumDescriptionLengthForPublishing} characters.");
        }
        if (_transportTimes.Count == 0)
        {
            throw new DomainException("A tour can be published only with at least one transport time.");
        }
        if (_keyPoints.Count < MinimumKeyPointsForPublishing)
        {
            throw new DomainException($"A tour can be published only with at least {MinimumKeyPointsForPublishing} key points.");
        }
    }

    private void EnsureKeyPointsCanChange()
    {
        if (Status != TourStatus.Draft)
        {
            throw new DomainException("Key points can be changed only while the tour is a draft.");
        }
    }

    private KeyPoint FindKeyPoint(Guid keyPointId)
    {
        return _keyPoints.FirstOrDefault(keyPoint => keyPoint.Id == keyPointId)
            ?? throw new NotFoundException($"Key point {keyPointId} does not exist.");
    }

    // The sum of the distances between consecutive key points, rounded to one decimal; no length below two key points.
    private void RecalculateLength()
    {
        var ordered = KeyPoints;
        if (ordered.Count < 2)
        {
            Length = null;
            return;
        }

        var kilometers = 0.0;
        for (var i = 1; i < ordered.Count; i++)
        {
            kilometers += ordered[i - 1].Location.DistanceTo(ordered[i].Location);
        }
        Length = new TourLength(Math.Round((decimal)kilometers, 1));
    }
}
