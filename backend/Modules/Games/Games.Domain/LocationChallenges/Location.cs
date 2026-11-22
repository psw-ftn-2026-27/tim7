using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed record Location
{
    private const double EarthRadiusMeters = 6_371_000;

    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public Location(double latitude, double longitude)
    {
        if (double.IsNaN(latitude) || latitude is < -90 or > 90)
        {
            throw new DomainException("The latitude must be between -90 and 90.");
        }
        if (double.IsNaN(longitude) || longitude is < -180 or > 180)
        {
            throw new DomainException("The longitude must be between -180 and 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    // Haversine distance over a spherical Earth, in meters.
    public double DistanceTo(Location other)
    {
        var latitude1 = ToRadians(Latitude);
        var latitude2 = ToRadians(other.Latitude);
        var deltaLatitude = latitude2 - latitude1;
        var deltaLongitude = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLatitude / 2) * Math.Sin(deltaLatitude / 2)
            + Math.Cos(latitude1) * Math.Cos(latitude2) * Math.Sin(deltaLongitude / 2) * Math.Sin(deltaLongitude / 2);

        return 2 * EarthRadiusMeters * Math.Asin(Math.Sqrt(a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
