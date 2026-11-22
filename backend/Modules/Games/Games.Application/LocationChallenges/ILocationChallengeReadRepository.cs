namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<List<LocationChallengeDto>> GetAllAsync();

    // Null when the challenge does not exist.
    Task<List<HiddenPointPhotoDto>?> GetHiddenPointPhotosAsync(Guid locationChallengeId);
}
