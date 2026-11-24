namespace Games.Application.QuizSolving;

// Exactly one of NextQuestion and Result is set: the next question while the quiz is unsolved, the result once it is solved.
public sealed record QuizSolvingDto(Guid Id, string Name, SolvingQuestionDto? NextQuestion, QuizResultDto? Result);

// Carries no correctness, so an unsolved quiz never reveals its correct answers.
public sealed record SolvingQuestionDto(Guid Id, string Text, List<SolvingAnswerOptionDto> AnswerOptions);

public sealed record SolvingAnswerOptionDto(Guid Id, string Text);

public sealed record QuizResultDto(int CorrectAnswerCount, int EarnedXp, List<AnsweredQuestionDto> Questions);

public sealed record AnsweredQuestionDto(
    Guid Id,
    string Text,
    SolvingAnswerOptionDto GivenAnswer,
    SolvingAnswerOptionDto CorrectAnswer);
