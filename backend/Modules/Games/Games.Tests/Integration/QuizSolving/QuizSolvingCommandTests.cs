using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallenges;
using Games.Application.QuizSolving;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizSolving;

public class QuizSolvingCommandTests : BaseIntegrationTest
{
    private const string Route = "/api/games/quizzes/solving";

    public QuizSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_activates_a_quiz_from_a_position_within_100_m()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var quiz = QuizSeed.PublishedRiversQuiz;
        var position = new LocationDto(quiz.Location!.Latitude, quiz.Location.Longitude);

        var response = await client.PostAsJsonAsync($"{Route}/{quiz.Id}/activate", position);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Should().ContainSingle(attempt =>
            attempt.UserId == WellKnownUsers.SecondExplorer && attempt.QuizId == quiz.Id);
    }

    [Fact]
    public async Task Explorer_cannot_activate_a_quiz_from_a_position_farther_than_100_m()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var quiz = QuizSeed.PublishedRiversQuiz;
        var positionAbout500MetersNorth = new LocationDto(quiz.Location!.Latitude + 0.0045, quiz.Location.Longitude);

        var response = await client.PostAsJsonAsync($"{Route}/{quiz.Id}/activate", positionAbout500MetersNorth);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Should().NotContain(attempt =>
            attempt.UserId == WellKnownUsers.SecondExplorer && attempt.QuizId == quiz.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_activate_a_quiz()
    {
        var client = Factory.CreateClient();
        var quiz = QuizSeed.PublishedRiversQuiz;
        var position = new LocationDto(quiz.Location!.Latitude, quiz.Location.Longitude);

        var response = await client.PostAsJsonAsync($"{Route}/{quiz.Id}/activate", position);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_activate_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = QuizSeed.PublishedRiversQuiz;
        var position = new LocationDto(quiz.Location!.Latitude, quiz.Location.Longitude);

        var response = await client.PostAsJsonAsync($"{Route}/{quiz.Id}/activate", position);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Explorer_answers_a_question_and_the_answer_is_recorded_in_the_attempt()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedRiversQuiz;
        var question = quiz.Questions[0];
        var answer = new AnswerDto(question.Id, question.AnswerOptions[0].Id);

        var response = await client.PostAsJsonAsync($"{Route}/{quiz.Id}/answers", answer);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var attempt = assertContext.QuizAttempts
            .Single(stored => stored.Id == QuizAttemptSeed.ExplorerStartedRiversQuiz.Id);
        attempt.GivenAnswers.Should().ContainSingle(given =>
            given.QuestionId == answer.QuestionId && given.AnswerOptionId == answer.AnswerOptionId);
    }

    [Fact]
    public async Task Explorer_cannot_answer_a_quiz_that_is_not_started()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var quiz = QuizSeed.PublishedRiversQuiz;
        var question = quiz.Questions[0];

        var response = await client.PostAsJsonAsync(
            $"{Route}/{quiz.Id}/answers", new AnswerDto(question.Id, question.AnswerOptions[0].Id));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Should().NotContain(attempt =>
            attempt.UserId == WellKnownUsers.SecondExplorer && attempt.QuizId == quiz.Id);
    }

    [Fact]
    public async Task Explorer_cannot_answer_the_same_question_again_and_the_given_answer_stays()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = QuizAttemptSeed.ExplorerHalfwayThroughFortressesQuiz;
        var givenAnswer = seeded.GivenAnswers[0];
        var question = QuizSeed.PublishedFortressesQuiz.Questions.Single(item => item.Id == givenAnswer.QuestionId);
        var otherOption = question.AnswerOptions.First(option => option.Id != givenAnswer.AnswerOptionId);

        var response = await client.PostAsJsonAsync(
            $"{Route}/{seeded.QuizId}/answers", new AnswerDto(question.Id, otherOption.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var attempt = assertContext.QuizAttempts.Single(stored => stored.Id == seeded.Id);
        attempt.GivenAnswers.Should().Equal(givenAnswer);
    }

    [Fact]
    public async Task Anonymous_user_cannot_answer_a_question()
    {
        var client = Factory.CreateClient();
        var quiz = QuizSeed.PublishedRiversQuiz;
        var question = quiz.Questions[0];

        var response = await client.PostAsJsonAsync(
            $"{Route}/{quiz.Id}/answers", new AnswerDto(question.Id, question.AnswerOptions[0].Id));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_answer_a_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = QuizSeed.PublishedRiversQuiz;
        var question = quiz.Questions[0];

        var response = await client.PostAsJsonAsync(
            $"{Route}/{quiz.Id}/answers", new AnswerDto(question.Id, question.AnswerOptions[0].Id));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
