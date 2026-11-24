using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class QuizAttempt : AggregateRoot
{
    private readonly List<GivenAnswer> _givenAnswers = [];

    public Guid UserId { get; private set; }
    public Guid QuizId { get; private set; }
    public IReadOnlyList<GivenAnswer> GivenAnswers => _givenAnswers;
    public DateTime? CompletedAt { get; private set; }
    public int? EarnedXp { get; private set; }

    private QuizAttempt() { }

    public QuizAttempt(Guid userId, Guid quizId) : base(Guid.NewGuid())
    {
        UserId = userId;
        QuizId = quizId;
    }

    public void EnsureCanActivate()
    {
        if (CompletedAt is not null)
        {
            throw new DomainException("A solved quiz cannot be activated again.");
        }
    }

    // The quiz tells the IDs of its questions in order and how much XP it brings.
    public void Answer(GivenAnswer answer, IReadOnlyList<Guid> questionIds, int quizXp)
    {
        if (_givenAnswers.Any(given => given.QuestionId == answer.QuestionId))
        {
            throw new DomainException("The question is already answered.");
        }

        var firstUnansweredId = questionIds.FirstOrDefault(questionId =>
            _givenAnswers.All(given => given.QuestionId != questionId));
        if (answer.QuestionId != firstUnansweredId)
        {
            throw new DomainException("Questions are answered in order; this is not the first unanswered question.");
        }

        _givenAnswers.Add(answer);

        if (_givenAnswers.Count == questionIds.Count)
        {
            CompletedAt = DateTime.UtcNow;
            EarnedXp = quizXp * _givenAnswers.Count(given => given.IsCorrect) / questionIds.Count;
        }
    }
}
