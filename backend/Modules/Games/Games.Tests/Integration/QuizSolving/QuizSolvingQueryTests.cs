using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizSolving;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizSolving;

public class QuizSolvingQueryTests : BaseIntegrationTest
{
    private const string Route = "/api/games/quizzes/solving";

    public QuizSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Unsolved_quiz_shows_the_next_question_without_the_correct_answers()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedRiversQuiz;

        var response = await client.GetAsync($"{Route}/{quiz.Id}");

        var body = await response.Content.ReadAsStringAsync();
        body.Should().NotContainEquivalentOf("correct");
        var view = await response.Content.ReadFromJsonAsync<QuizSolvingDto>(JsonOptions);
        view!.Result.Should().BeNull();
        view.NextQuestion!.Id.Should().Be(quiz.Questions[0].Id);
    }

    [Fact]
    public async Task Solved_quiz_shows_the_given_and_the_correct_answer_of_every_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var quiz = QuizSeed.PublishedFortressesQuiz;
        var attempt = QuizAttemptSeed.SecondExplorerSolvedFortressesQuiz;
        var expectedQuestions = quiz.Questions.Select(question => new
        {
            question.Id,
            GivenAnswer = new { Id = attempt.GivenAnswers.Single(answer => answer.QuestionId == question.Id).AnswerOptionId },
            CorrectAnswer = new { question.AnswerOptions.Single(option => option.IsCorrect).Id }
        });

        var view = await client.GetFromJsonAsync<QuizSolvingDto>($"{Route}/{quiz.Id}", JsonOptions);

        view!.NextQuestion.Should().BeNull();
        view.Result!.CorrectAnswerCount.Should().Be(attempt.GivenAnswers.Count(answer => answer.IsCorrect));
        view.Result.EarnedXp.Should().Be(attempt.EarnedXp);
        view.Result.Questions.Should().BeEquivalentTo(expectedQuestions);
    }
}
