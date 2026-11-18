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

    private AnswerOption GetAnswerOption(Guid answerOptionId)
    {
        return _answerOptions.FirstOrDefault(answerOption => answerOption.Id == answerOptionId)
            ?? throw new DomainException($"The question has no answer option {answerOptionId}.");
    }
}
