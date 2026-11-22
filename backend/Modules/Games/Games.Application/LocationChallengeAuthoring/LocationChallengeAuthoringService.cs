using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Shared.Domain.Images;

namespace Games.Application.LocationChallengeAuthoring;

public sealed class LocationChallengeAuthoringService
{
    private readonly ILocationChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStore _imageStore;

    public LocationChallengeAuthoringService(
        ILocationChallengeRepository challengeRepository,
        IUnitOfWork unitOfWork,
        IImageStore imageStore)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
        _imageStore = imageStore;
    }

    public async Task<Guid> CreateAsync(CreateLocationChallengeDto dto, List<ImageUploadDto> images)
    {
        var addresses = await SaveImagesAsync(images);

        // The photo at position i belongs to the point at position i; a point without a photo gets an empty
        // address, which HiddenPoint rejects.
        var hiddenPoints = (dto.HiddenPoints ?? [])
            .Select((point, i) => new HiddenPoint(
                i < addresses.Count ? addresses[i] : "",
                ToLocation(point.Location)))
            .ToList();

        // The radius bonus is random; the aggregate only checks its range.
        var radiusBonusMeters = Random.Shared.Next(0, 101);

        var challenge = new LocationChallenge(
            dto.Name, dto.Xp, ToLocation(dto.StartLocation), hiddenPoints, radiusBonusMeters);

        _challengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return challenge.Id;
    }

    private static Location? ToLocation(LocationDto? dto)
    {
        return dto is null ? null : new Location(dto.Latitude, dto.Longitude);
    }

    // ImageFile and ImageSelection check every image before anything is written to disk.
    // A file saved for a challenge that the domain then rejects is removed later by the platform's image cleanup job.
    private async Task<List<string>> SaveImagesAsync(List<ImageUploadDto> uploads)
    {
        var files = uploads
            .Select(upload => new ImageFile(upload.FileName, upload.Content, upload.Width, upload.Height))
            .ToList();

        return await _imageStore.SaveAsync(new ImageSelection(files));
    }
}
