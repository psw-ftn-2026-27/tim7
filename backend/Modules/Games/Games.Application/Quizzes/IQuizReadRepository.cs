using Games.Application.QuizManagement;
using Games.Application.QuizSolving;
using Shared.Domain;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<PageResult<ChallengeListItemDto>> GetChallengesAsync(int page, int pageSize);
    Task<QuizDto?> GetByIdAsync(Guid id);
    Task<List<PublishedQuizDto>> GetPublishedAsync();
}
