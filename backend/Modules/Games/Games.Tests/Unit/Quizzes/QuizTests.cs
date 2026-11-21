using FluentAssertions;
using Games.Domain.LocationChallenges;
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

    private static readonly Location Belgrade = new(44.8176, 20.4569);

    private static Quiz CreateCompleteQuizWithoutLocation()
    {
        var quiz = new Quiz("Prestonice Evrope", "Izaberi tačan odgovor.", 50);
        AddCompleteQuestion(quiz, "Glavni grad Srbije?", "Beograd", "Novi Sad");
        return quiz;
    }

    private static Quiz CreateCompleteQuiz()
    {
        var quiz = CreateCompleteQuizWithoutLocation();
        quiz.SetLocation(Belgrade);
        return quiz;
    }

    private static void AddCompleteQuestion(Quiz quiz, string text, string correctAnswer, string wrongAnswer)
    {
        quiz.AddQuestion(text);
        var question = quiz.Questions[^1];
        quiz.AddAnswerOption(question.Id, correctAnswer);
        quiz.AddAnswerOption(question.Id, wrongAnswer);
        quiz.MarkAnswerOptionCorrect(question.Id, question.AnswerOptions[0].Id);
    }

    private static Quiz CreateQuizIn(QuizStatus status)
    {
        var quiz = CreateCompleteQuiz();
        if (status != QuizStatus.Draft)
        {
            quiz.Publish();
        }
        if (status is QuizStatus.Retired or QuizStatus.Archived)
        {
            quiz.Retire();
        }
        if (status == QuizStatus.Archived)
        {
            quiz.Archive();
        }
        return quiz;
    }

    private static Action Transition(Quiz quiz, string transition)
    {
        return transition switch
        {
            "publish" => quiz.Publish,
            "retire" => quiz.Retire,
            "restore" => quiz.Restore,
            "archive" => quiz.Archive,
            _ => throw new ArgumentOutOfRangeException(nameof(transition))
        };
    }

    private static Action Modification(Quiz quiz, string modification)
    {
        var question = quiz.Questions[0];
        return modification switch
        {
            "update details" => () => quiz.UpdateDetails("Novi naziv", "Nova uputstva.", 80),
            "add question" => () => quiz.AddQuestion("Glavni grad Francuske?"),
            "remove question" => () => quiz.RemoveQuestion(question.Id),
            "add answer option" => () => quiz.AddAnswerOption(question.Id, "Niš"),
            "remove answer option" => () => quiz.RemoveAnswerOption(question.Id, question.AnswerOptions[1].Id),
            "mark answer option correct" => () => quiz.MarkAnswerOptionCorrect(question.Id, question.AnswerOptions[1].Id),
            "set location" => () => quiz.SetLocation(new Location(45.2551, 19.8452)),
            _ => throw new ArgumentOutOfRangeException(nameof(modification))
        };
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

    [Fact]
    public void Complete_draft_quiz_is_published()
    {
        var quiz = CreateCompleteQuiz();

        quiz.Publish();

        quiz.Status.Should().Be(QuizStatus.Published);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Quiz_without_instructions_is_not_published(string? instructions)
    {
        var quiz = CreateCompleteQuiz();
        quiz.UpdateDetails("Prestonice Evrope", instructions, 50);

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Quiz_without_xp_is_not_published()
    {
        var quiz = CreateCompleteQuiz();
        quiz.UpdateDetails("Prestonice Evrope", "Izaberi tačan odgovor.", null);

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Quiz_without_a_location_is_not_published()
    {
        var quiz = CreateCompleteQuizWithoutLocation();

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Draft_quiz_gets_a_location()
    {
        var quiz = CreateCompleteQuizWithoutLocation();

        quiz.SetLocation(Belgrade);

        quiz.Location.Should().Be(Belgrade);
    }

    [Fact]
    public void Location_of_a_draft_quiz_is_changed()
    {
        var quiz = CreateCompleteQuiz();
        var noviSad = new Location(45.2551, 19.8452);

        quiz.SetLocation(noviSad);

        quiz.Location.Should().Be(noviSad);
    }

    [Fact]
    public void Quiz_without_questions_is_not_published()
    {
        var quiz = CreateCompleteQuiz();
        quiz.RemoveQuestion(quiz.Questions[0].Id);

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Quiz_with_a_question_that_has_fewer_than_two_answer_options_is_not_published()
    {
        var quiz = CreateCompleteQuiz();
        var question = quiz.Questions[0];
        quiz.RemoveAnswerOption(question.Id, question.AnswerOptions[1].Id);

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public void Quiz_with_a_question_without_a_correct_answer_option_is_not_published()
    {
        var quiz = CreateCompleteQuiz();
        quiz.AddQuestion("Glavni grad Francuske?");
        var questionId = quiz.Questions[1].Id;
        quiz.AddAnswerOption(questionId, "Pariz");
        quiz.AddAnswerOption(questionId, "Lion");

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Quiz_with_a_question_without_text_is_not_published(string text)
    {
        var quiz = CreateCompleteQuiz();
        AddCompleteQuestion(quiz, text, "Pariz", "Lion");

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Quiz_with_an_answer_option_without_text_is_not_published(string text)
    {
        var quiz = CreateCompleteQuiz();
        AddCompleteQuestion(quiz, "Glavni grad Francuske?", "Pariz", text);

        var publication = () => quiz.Publish();

        publication.Should().Throw<DomainException>();
        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [InlineData(QuizStatus.Published, "retire", QuizStatus.Retired)]
    [InlineData(QuizStatus.Retired, "restore", QuizStatus.Published)]
    [InlineData(QuizStatus.Retired, "archive", QuizStatus.Archived)]
    public void Allowed_transition_changes_the_status_of_a_quiz(QuizStatus from, string transition, QuizStatus to)
    {
        var quiz = CreateQuizIn(from);

        Transition(quiz, transition)();

        quiz.Status.Should().Be(to);
    }

    [Theory]
    [InlineData(QuizStatus.Draft, "retire")]
    [InlineData(QuizStatus.Draft, "restore")]
    [InlineData(QuizStatus.Draft, "archive")]
    [InlineData(QuizStatus.Published, "publish")]
    [InlineData(QuizStatus.Published, "restore")]
    [InlineData(QuizStatus.Published, "archive")]
    [InlineData(QuizStatus.Retired, "publish")]
    [InlineData(QuizStatus.Retired, "retire")]
    [InlineData(QuizStatus.Archived, "publish")]
    [InlineData(QuizStatus.Archived, "retire")]
    [InlineData(QuizStatus.Archived, "restore")]
    [InlineData(QuizStatus.Archived, "archive")]
    public void Transition_that_is_not_allowed_leaves_the_quiz_in_its_status(QuizStatus status, string transition)
    {
        var quiz = CreateQuizIn(status);

        var change = Transition(quiz, transition);

        change.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Archived)]
    public void Draft_or_archived_quiz_can_be_deleted(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var deletion = () => quiz.EnsureDeletable();

        deletion.Should().NotThrow();
    }

    [Theory]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Retired)]
    public void Published_or_retired_quiz_cannot_be_deleted(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var deletion = () => quiz.EnsureDeletable();

        deletion.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("update details")]
    [InlineData("add question")]
    [InlineData("remove question")]
    [InlineData("add answer option")]
    [InlineData("remove answer option")]
    [InlineData("mark answer option correct")]
    [InlineData("set location")]
    public void Quiz_that_is_not_a_draft_cannot_be_changed(string modification)
    {
        var quiz = CreateQuizIn(QuizStatus.Published);

        var change = Modification(quiz, modification);

        change.Should().Throw<DomainException>();
        quiz.Name.Should().Be("Prestonice Evrope");
        quiz.Instructions.Should().Be("Izaberi tačan odgovor.");
        quiz.Xp.Should().Be(50);
        quiz.Questions.Should().ContainSingle();
        quiz.Questions[0].AnswerOptions.Should().HaveCount(2);
        quiz.Questions[0].AnswerOptions[0].IsCorrect.Should().BeTrue();
        quiz.Questions[0].AnswerOptions[1].IsCorrect.Should().BeFalse();
        quiz.Location.Should().Be(Belgrade);
    }
}
