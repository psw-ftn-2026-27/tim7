using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizTests
{
    private static Quiz CreateQuizWithOneQuestion()
    {
        var quiz = new Quiz("Prestonice Evrope", "Izaberi tačan odgovor.", 50);
        quiz.AddQuestion("Glavni grad Srbije?");
        return quiz;
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Quiz_cannot_be_created_without_a_name(string name)
    {
        var creation = () => new Quiz(name, "Uputstva.", 50);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_of_a_quiz_cannot_be_emptied(string name)
    {
        var quiz = CreateQuizWithOneQuestion();

        var update = () => quiz.UpdateDetails(name, "Uputstva.", 50);

        update.Should().Throw<DomainException>();
        quiz.Name.Should().Be("Prestonice Evrope");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Quiz_cannot_be_created_with_xp_that_is_not_positive(int xp)
    {
        var creation = () => new Quiz("Prestonice Evrope", "Uputstva.", xp);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Xp_of_a_quiz_cannot_be_changed_to_a_value_that_is_not_positive(int xp)
    {
        var quiz = CreateQuizWithOneQuestion();

        var update = () => quiz.UpdateDetails("Prestonice Evrope", "Uputstva.", xp);

        update.Should().Throw<DomainException>();
        quiz.Xp.Should().Be(50);
    }

    [Fact]
    public void Marking_an_answer_option_correct_unmarks_the_previous_correct_one()
    {
        var quiz = CreateQuizWithOneQuestion();
        var questionId = quiz.Questions[0].Id;
        quiz.AddAnswerOption(questionId, "Beograd");
        quiz.AddAnswerOption(questionId, "Novi Sad");
        var belgrade = quiz.Questions[0].AnswerOptions[0];
        var noviSad = quiz.Questions[0].AnswerOptions[1];
        quiz.MarkAnswerOptionCorrect(questionId, belgrade.Id);

        quiz.MarkAnswerOptionCorrect(questionId, noviSad.Id);

        noviSad.IsCorrect.Should().BeTrue();
        belgrade.IsCorrect.Should().BeFalse();
    }

    [Fact]
    public void Draft_quiz_can_be_composed_unfinished()
    {
        var quiz = new Quiz("Prestonice Evrope", null, null);
        quiz.AddQuestion("");
        var questionId = quiz.Questions[0].Id;

        quiz.AddAnswerOption(questionId, "");

        quiz.Instructions.Should().BeNull();
        quiz.Xp.Should().BeNull();
        quiz.Questions.Should().ContainSingle();
        quiz.Questions[0].AnswerOptions.Should().ContainSingle();
        quiz.Questions[0].AnswerOptions.Should().NotContain(option => option.IsCorrect);
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Question_that_is_not_in_the_quiz_cannot_be_removed()
    {
        var quiz = CreateQuizWithOneQuestion();

        var removal = () => quiz.RemoveQuestion(Guid.NewGuid());

        removal.Should().Throw<DomainException>();
        quiz.Questions.Should().ContainSingle();
    }

    [Fact]
    public void Answer_option_cannot_be_added_to_a_question_that_is_not_in_the_quiz()
    {
        var quiz = CreateQuizWithOneQuestion();

        var addition = () => quiz.AddAnswerOption(Guid.NewGuid(), "Beograd");

        addition.Should().Throw<DomainException>();
        quiz.Questions[0].AnswerOptions.Should().BeEmpty();
    }

    [Fact]
    public void Answer_option_that_is_not_in_the_question_cannot_be_removed()
    {
        var quiz = CreateQuizWithOneQuestion();
        var questionId = quiz.Questions[0].Id;
        quiz.AddAnswerOption(questionId, "Beograd");

        var removal = () => quiz.RemoveAnswerOption(questionId, Guid.NewGuid());

        removal.Should().Throw<DomainException>();
        quiz.Questions[0].AnswerOptions.Should().ContainSingle();
    }

    [Fact]
    public void Answer_option_that_is_not_in_the_question_cannot_be_marked_correct()
    {
        var quiz = CreateQuizWithOneQuestion();
        var questionId = quiz.Questions[0].Id;
        quiz.AddAnswerOption(questionId, "Beograd");
        quiz.MarkAnswerOptionCorrect(questionId, quiz.Questions[0].AnswerOptions[0].Id);

        var marking = () => quiz.MarkAnswerOptionCorrect(questionId, Guid.NewGuid());

        marking.Should().Throw<DomainException>();
        quiz.Questions[0].AnswerOptions[0].IsCorrect.Should().BeTrue();
    }

    [Fact]
    public void Question_that_is_not_in_the_quiz_cannot_have_an_answer_option_marked_correct()
    {
        var quiz = CreateQuizWithOneQuestion();

        var marking = () => quiz.MarkAnswerOptionCorrect(Guid.NewGuid(), Guid.NewGuid());

        marking.Should().Throw<DomainException>();
    }
}
