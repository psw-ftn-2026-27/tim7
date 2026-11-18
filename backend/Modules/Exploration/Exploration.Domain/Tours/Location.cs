using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record Location
{
    private const double EarthRadiusInKilometers = 6371;

    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("A latitude must be between -90 and 90 degrees.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("A longitude must be between -180 and 180 degrees.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    // Air (great-circle) distance by the haversine formula, the same one the frontend's shared map uses.
    public double DistanceTo(Location other)
    {
        var deltaLatitude = ToRadians(other.Latitude - Latitude);
        var deltaLongitude = ToRadians(other.Longitude - Longitude);
        var a = Math.Pow(Math.Sin(deltaLatitude / 2), 2)
            + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) * Math.Pow(Math.Sin(deltaLongitude / 2), 2);

        return 2 * EarthRadiusInKilometers * Math.Asin(Math.Sqrt(a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
