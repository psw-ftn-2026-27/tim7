using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

// An inner entity of the Tour aggregate: created, changed and ordered only by its tour.
public sealed class KeyPoint : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Image { get; private set; }
    public Location Location { get; private set; }
    public string? Secret { get; private set; }
    public bool IsPublic { get; private set; }
    public int Order { get; private set; }

    private KeyPoint()
    {
        Name = null!;
        Description = null!;
        Image = null!;
        Location = null!;
    }

    internal KeyPoint(string name, string description, string image, Location location, string? secret, bool isPublic, int order)
        : base(Guid.NewGuid())
    {
        EnsureRequiredDataIsPresent(name, description, image, location);

        Name = name;
        Description = description;
        Image = image;
        Location = location;
        Secret = secret;
        IsPublic = isPublic;
        Order = order;
    }

    internal void Change(string name, string description, string image, Location location, string? secret, bool isPublic)
    {
        EnsureRequiredDataIsPresent(name, description, image, location);

        Name = name;
        Description = description;
        Image = image;
        Location = location;
        Secret = secret;
        IsPublic = isPublic;
    }

    internal void MoveTo(int order)
    {
        Order = order;
    }

    private static void EnsureRequiredDataIsPresent(string name, string description, string image, Location location)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A key point requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A key point requires a description.");
        }
        if (string.IsNullOrWhiteSpace(image))
        {
            throw new DomainException("A key point requires an image.");
        }
        if (location is null)
        {
            throw new DomainException("A key point requires a location.");
        }
    }
}
