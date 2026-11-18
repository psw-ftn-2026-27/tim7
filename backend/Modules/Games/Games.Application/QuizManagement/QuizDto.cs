using Games.Domain.Quizzes;

namespace Games.Application.QuizManagement;

public sealed record QuizDto(
    Guid Id,
    string Name,
    string? Instructions,
    int? Xp,
    QuizStatus Status,
    List<QuestionDto> Questions);

public sealed record QuestionDto(Guid Id, string Text, List<AnswerOptionDto> AnswerOptions);

public sealed record AnswerOptionDto(Guid Id, string Text, bool IsCorrect);
