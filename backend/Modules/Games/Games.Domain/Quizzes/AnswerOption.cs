using Shared.Domain;

namespace Games.Domain.Quizzes;

public sealed class AnswerOption : Entity
{
    public string Text { get; private set; }
    public bool IsCorrect { get; private set; }

    private AnswerOption()
    {
        Text = null!;
    }

    internal AnswerOption(string text) : base(Guid.NewGuid())
    {
        Text = text;
    }

    internal void SetCorrect(bool isCorrect)
    {
        IsCorrect = isCorrect;
    }
}
