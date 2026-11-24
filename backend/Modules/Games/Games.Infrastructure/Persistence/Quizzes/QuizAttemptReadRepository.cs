using Games.Application.Quizzes;
using Games.Application.QuizSolving;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizAttemptReadRepository : IQuizAttemptReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizAttemptReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<QuizAttemptDto>> GetAllOfUserAsync(Guid userId)
    {
        return _dbContext.QuizAttempts
            .Where(attempt => attempt.UserId == userId)
            .Select(attempt => new QuizAttemptDto(attempt.QuizId, attempt.CompletedAt != null))
            .ToListAsync();
    }

    public Task<QuizAttemptDetailsDto?> GetAsync(Guid userId, Guid quizId)
    {
        return _dbContext.QuizAttempts
            .Where(attempt => attempt.UserId == userId && attempt.QuizId == quizId)
            .Select(attempt => new QuizAttemptDetailsDto(
                attempt.CompletedAt != null,
                attempt.EarnedXp,
                attempt.GivenAnswers
                    .Select(answer => new GivenAnswerDto(answer.QuestionId, answer.AnswerOptionId, answer.IsCorrect))
                    .ToList()))
            .FirstOrDefaultAsync();
    }
}
