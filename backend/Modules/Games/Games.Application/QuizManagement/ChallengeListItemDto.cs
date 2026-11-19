using Games.Domain.Quizzes;

namespace Games.Application.QuizManagement;

public sealed record ChallengeListItemDto(Guid Id, string Name, string Kind, QuizStatus Status, int? Xp, int QuestionCount);
