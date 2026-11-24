using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizAttemptRepository : IQuizAttemptRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizAttemptRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<QuizAttempt?> GetAsync(Guid userId, Guid quizId)
    {
        return _dbContext.QuizAttempts.FirstOrDefaultAsync(attempt =>
            attempt.UserId == userId && attempt.QuizId == quizId);
    }

    public void Add(QuizAttempt attempt)
    {
        _dbContext.QuizAttempts.Add(attempt);
    }
}
