namespace Games.Application.QuizSolving;

public sealed record QuizAttemptDto(Guid QuizId, bool IsCompleted);
