using Games.Application.Quizzes;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizManagement;

public sealed class QuizManagementQueries
{
    private readonly IQuizReadRepository _quizReadRepository;

    public QuizManagementQueries(IQuizReadRepository quizReadRepository)
    {
        _quizReadRepository = quizReadRepository;
    }

    public Task<PageResult<ChallengeListItemDto>> GetChallengesAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _quizReadRepository.GetChallengesAsync(page, pageSize);
    }

    public async Task<QuizDto> GetQuizAsync(Guid quizId)
    {
        var quiz = await _quizReadRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        return quiz;
    }
}
