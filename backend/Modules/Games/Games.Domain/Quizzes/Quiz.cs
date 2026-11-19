using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    private readonly List<Question> _questions = [];

    public string Name { get; private set; }
    public string? Instructions { get; private set; }
    public int? Xp { get; private set; }
    public QuizStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;

    private Quiz()
    {
        Name = null!;
    }

    public Quiz(string name, string? instructions, int? xp) : base(Guid.NewGuid())
    {
        ValidateDetails(name, xp);

        Name = name;
        Instructions = instructions;
        Xp = xp;
        Status = QuizStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? instructions, int? xp)
    {
        EnsureDraft();
        ValidateDetails(name, xp);

        Name = name;
        Instructions = instructions;
        Xp = xp;
    }

    public void AddQuestion(string text)
    {
        EnsureDraft();

        _questions.Add(new Question(text));
    }

    public void RemoveQuestion(Guid questionId)
    {
        EnsureDraft();

        _questions.Remove(GetQuestion(questionId));
    }

    public void AddAnswerOption(Guid questionId, string text)
    {
        EnsureDraft();

        GetQuestion(questionId).AddAnswerOption(text);
    }

    public void RemoveAnswerOption(Guid questionId, Guid answerOptionId)
    {
        EnsureDraft();

        GetQuestion(questionId).RemoveAnswerOption(answerOptionId);
    }

    public void MarkAnswerOptionCorrect(Guid questionId, Guid answerOptionId)
    {
        EnsureDraft();

        GetQuestion(questionId).MarkCorrect(answerOptionId);
    }

    public void Publish()
    {
        if (Status != QuizStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be published.");
        }
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Instructions))
        {
            throw new DomainException("A quiz needs a name and instructions to be published.");
        }
        if (Xp is not > 0)
        {
            throw new DomainException("A quiz needs XP greater than zero to be published.");
        }
        if (_questions.Count == 0)
        {
            throw new DomainException("A quiz needs at least one question to be published.");
        }
        foreach (var question in _questions)
        {
            question.EnsureReadyForPublish();
        }

        Status = QuizStatus.Published;
    }

    public void Retire()
    {
        if (Status != QuizStatus.Published)
        {
            throw new DomainException("Only a published quiz can be retired.");
        }

        Status = QuizStatus.Retired;
    }

    public void Restore()
    {
        if (Status != QuizStatus.Retired)
        {
            throw new DomainException("Only a retired quiz can be restored.");
        }

        Status = QuizStatus.Published;
    }

    public void Archive()
    {
        if (Status != QuizStatus.Retired)
        {
            throw new DomainException("Only a retired quiz can be archived.");
        }

        Status = QuizStatus.Archived;
    }

    public void EnsureDeletable()
    {
        if (Status != QuizStatus.Draft && Status != QuizStatus.Archived)
        {
            throw new DomainException("Only a draft or an archived quiz can be deleted.");
        }
    }

    private static void ValidateDetails(string name, int? xp)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A quiz requires a name.");
        }
        if (xp is <= 0)
        {
            throw new DomainException("Quiz XP, when given, must be greater than zero.");
        }
    }

    private void EnsureDraft()
    {
        if (Status != QuizStatus.Draft)
        {
            throw new DomainException("A quiz can be changed only while it is a draft.");
        }
    }

    private Question GetQuestion(Guid questionId)
    {
        return _questions.FirstOrDefault(question => question.Id == questionId)
            ?? throw new DomainException($"The quiz has no question {questionId}.");
    }
}
