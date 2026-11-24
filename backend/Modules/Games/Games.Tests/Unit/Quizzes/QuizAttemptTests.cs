using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizAttemptTests
{
    private const int QuizXp = 50;

    private static readonly IReadOnlyList<Guid> QuestionIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

    private static QuizAttempt NewAttempt() => new(Guid.NewGuid(), Guid.NewGuid());

    private static GivenAnswer AnswerTo(Guid questionId, bool isCorrect) => new(questionId, Guid.NewGuid(), isCorrect);

    [Fact]
    public void Solved_quiz_cannot_be_activated_again()
    {
        var attempt = NewAttempt();
        var questionId = Guid.NewGuid();
        attempt.Answer(AnswerTo(questionId, true), [questionId], QuizXp);

        var activation = () => attempt.EnsureCanActivate();

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Second_answer_to_the_same_question_is_rejected_and_the_given_answer_stays()
    {
        var attempt = NewAttempt();
        var firstAnswer = AnswerTo(QuestionIds[0], true);
        attempt.Answer(firstAnswer, QuestionIds, QuizXp);

        var answering = () => attempt.Answer(AnswerTo(QuestionIds[0], false), QuestionIds, QuizXp);

        answering.Should().Throw<DomainException>();
        attempt.GivenAnswers.Should().Equal(firstAnswer);
    }

    [Fact]
    public void Answer_to_a_question_that_is_not_the_first_unanswered_one_is_rejected()
    {
        var attempt = NewAttempt();

        var answering = () => attempt.Answer(AnswerTo(QuestionIds[1], true), QuestionIds, QuizXp);

        answering.Should().Throw<DomainException>();
        attempt.GivenAnswers.Should().BeEmpty();
    }

    [Fact]
    public void Answering_the_last_question_solves_the_quiz_and_records_the_xp_rounded_down()
    {
        var attempt = NewAttempt();
        attempt.Answer(AnswerTo(QuestionIds[0], true), QuestionIds, QuizXp);
        attempt.Answer(AnswerTo(QuestionIds[1], false), QuestionIds, QuizXp);

        attempt.Answer(AnswerTo(QuestionIds[2], true), QuestionIds, QuizXp);

        attempt.CompletedAt.Should().NotBeNull();
        attempt.EarnedXp.Should().Be(33);
    }
}
