namespace Games.Application.QuizSolving;

public sealed record QuizAttemptDetailsDto(bool IsCompleted, int? EarnedXp, List<GivenAnswerDto> GivenAnswers);

public sealed record GivenAnswerDto(Guid QuestionId, Guid AnswerOptionId, bool IsCorrect);
