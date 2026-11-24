using Games.Application.QuizSolving;

namespace Games.Application.Quizzes;

public interface IQuizAttemptReadRepository
{
    Task<List<QuizAttemptDto>> GetAllOfUserAsync(Guid userId);

    // Null when the user has not started the quiz.
    Task<QuizAttemptDetailsDto?> GetAsync(Guid userId, Guid quizId);
}
