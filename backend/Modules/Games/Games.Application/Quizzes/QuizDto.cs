using Games.Application.LocationChallenges;
using Games.Domain.Quizzes;

namespace Games.Application.Quizzes;

public sealed record QuizDto(
    Guid Id,
    string Name,
    string? Instructions,
    int? Xp,
    QuizStatus Status,
    LocationDto? Location,
    List<QuestionDto> Questions);

public sealed record QuestionDto(Guid Id, string Text, List<AnswerOptionDto> AnswerOptions);

public sealed record AnswerOptionDto(Guid Id, string Text, bool IsCorrect);
