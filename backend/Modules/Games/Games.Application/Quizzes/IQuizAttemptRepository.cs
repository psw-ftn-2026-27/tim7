using Games.Domain.Quizzes;

namespace Games.Application.Quizzes;

public interface IQuizAttemptRepository
{
    Task<QuizAttempt?> GetAsync(Guid userId, Guid quizId);
    void Add(QuizAttempt attempt);
}
