using Exploration.Application.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.KeyPointAuthoring;

public sealed class KeyPointAuthoringQueries
{
    private readonly ITourReadRepository _tourReadRepository;

    public KeyPointAuthoringQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    // Another author's tour and a missing tour give the same answer, as the ownership check of the commands does.
    public async Task<AuthorTourDto> GetAuthorTourAsync(Guid tourId, Guid authorId)
    {
        return await _tourReadRepository.GetAuthorTourAsync(tourId, authorId)
            ?? throw new NotFoundException($"Tour {tourId} does not exist.");
    }
}
