namespace Games.Domain.Quizzes;

// One answer an explorer gave. It refers to the quiz's question and answer option by ID only.
public sealed record GivenAnswer
{
    public Guid QuestionId { get; private init; }
    public Guid AnswerOptionId { get; private init; }
    public bool IsCorrect { get; private init; }

    private GivenAnswer() { }

    public GivenAnswer(Guid questionId, Guid answerOptionId, bool isCorrect)
    {
        QuestionId = questionId;
        AnswerOptionId = answerOptionId;
        IsCorrect = isCorrect;
    }
}
