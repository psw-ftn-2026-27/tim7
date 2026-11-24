namespace Games.Application.QuizSolving;

public sealed record SolvableQuizDto(Guid Id, string Name, QuizSolvingState State);
