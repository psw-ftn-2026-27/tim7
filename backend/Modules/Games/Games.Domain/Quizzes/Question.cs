using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Question : Entity
{
    private readonly List<AnswerOption> _answerOptions = [];

    public string Text { get; private set; }
    public IReadOnlyList<AnswerOption> AnswerOptions => _answerOptions;

    private Question()
    {
        Text = null!;
    }

    internal Question(string text) : base(Guid.NewGuid())
    {
        Text = text;
    }

    internal void AddAnswerOption(string text)
    {
        _answerOptions.Add(new AnswerOption(text));
    }

    internal void RemoveAnswerOption(Guid answerOptionId)
    {
        _answerOptions.Remove(GetAnswerOption(answerOptionId));
    }

    internal void MarkCorrect(Guid answerOptionId)
    {
        var correct = GetAnswerOption(answerOptionId);

        foreach (var answerOption in _answerOptions)
        {
            answerOption.SetCorrect(false);
        }
        correct.SetCorrect(true);
    }

    internal void EnsureReadyForPublish()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            throw new DomainException("Every question needs a text before the quiz is published.");
        }
        if (_answerOptions.Count < 2)
        {
            throw new DomainException($"The question \"{Text}\" needs at least two answer options.");
        }
        if (_answerOptions.Count(answerOption => answerOption.IsCorrect) != 1)
        {
            throw new DomainException($"The question \"{Text}\" needs exactly one correct answer option.");
        }
        if (_answerOptions.Any(answerOption => string.IsNullOrWhiteSpace(answerOption.Text)))
        {
            throw new DomainException($"Every answer option of the question \"{Text}\" needs a text.");
        }
    }

    private AnswerOption GetAnswerOption(Guid answerOptionId)
    {
        return _answerOptions.FirstOrDefault(answerOption => answerOption.Id == answerOptionId)
            ?? throw new DomainException($"The question has no answer option {answerOptionId}.");
    }
}
