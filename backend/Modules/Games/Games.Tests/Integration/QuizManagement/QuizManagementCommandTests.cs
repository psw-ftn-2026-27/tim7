using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizManagement;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Microsoft.EntityFrameworkCore;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizManagement;

public class QuizManagementCommandTests : BaseIntegrationTest
{
    private const string BaseUrl = "/api/games/quizzes";

    public QuizManagementCommandTests(GamesApiFactory factory) : base(factory) { }

    public static IEnumerable<object[]> Commands()
    {
        var quizUrl = $"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}";
        var questionUrl = $"{quizUrl}/questions/{QuizSeed.CapitalsQuiz.Questions[0].Id}";
        var answerUrl = $"{questionUrl}/answer-options/{QuizSeed.CapitalsQuiz.Questions[0].AnswerOptions[0].Id}";

        yield return [HttpMethod.Post.Method, BaseUrl];
        yield return [HttpMethod.Put.Method, quizUrl];
        yield return [HttpMethod.Post.Method, $"{quizUrl}/questions"];
        yield return [HttpMethod.Delete.Method, questionUrl];
        yield return [HttpMethod.Post.Method, $"{questionUrl}/answer-options"];
        yield return [HttpMethod.Delete.Method, answerUrl];
        yield return [HttpMethod.Post.Method, $"{answerUrl}/mark-correct"];
    }

    private static Quiz LoadQuiz(GamesDbContext context, Guid quizId)
    {
        return context.Quizzes
            .Include(quiz => quiz.Questions)
            .ThenInclude(question => question.AnswerOptions)
            .Single(quiz => quiz.Id == quizId);
    }

    [Fact]
    public async Task Administrator_creates_a_draft_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new QuizDetailsDto("Novi kviz", "Uputstva za novi kviz.", 30);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore + 1);
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == createdId);
        stored.Name.Should().Be("Novi kviz");
        stored.Instructions.Should().Be("Uputstva za novi kviz.");
        stored.Xp.Should().Be(30);
        stored.Status.Should().Be(QuizStatus.Draft);
        stored.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Administrator_cannot_create_a_quiz_without_a_name()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new QuizDetailsDto("   ", "Uputstva.", 30);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync(BaseUrl, request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
    }

    [Fact]
    public async Task Administrator_saves_the_details_of_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new QuizDetailsDto("Preimenovan kviz", "Nova uputstva.", 80);

        var response = await client.PutAsJsonAsync($"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = LoadQuiz(assertContext, QuizSeed.CapitalsQuiz.Id);
        stored.Name.Should().Be("Preimenovan kviz");
        stored.Instructions.Should().Be("Nova uputstva.");
        stored.Xp.Should().Be(80);
    }

    [Fact]
    public async Task Administrator_adds_a_question_to_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new TextDto("Glavni grad Francuske?");

        var response = await client.PostAsJsonAsync($"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}/questions", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = LoadQuiz(assertContext, QuizSeed.CapitalsQuiz.Id);
        stored.Questions.Should().ContainSingle(question => question.Text == "Glavni grad Francuske?");
    }

    [Fact]
    public async Task Administrator_removes_a_question_with_its_answer_options()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var question = QuizSeed.CapitalsQuiz.Questions[0];

        var response = await client.DeleteAsync($"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}/questions/{question.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = LoadQuiz(assertContext, QuizSeed.CapitalsQuiz.Id);
        stored.Questions.Should().BeEmpty();
    }

    [Fact]
    public async Task Administrator_adds_an_answer_option_to_a_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var question = QuizSeed.DraftWithEmptyQuestion.Questions[0];
        var request = new TextDto("Prvi odgovor");

        var response = await client.PostAsJsonAsync(
            $"{BaseUrl}/{QuizSeed.DraftWithEmptyQuestion.Id}/questions/{question.Id}/answer-options", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = LoadQuiz(assertContext, QuizSeed.DraftWithEmptyQuestion.Id);
        stored.Questions[0].AnswerOptions.Should().ContainSingle(option => option.Text == "Prvi odgovor");
    }

    [Fact]
    public async Task Administrator_removes_an_answer_option_from_a_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var question = QuizSeed.CapitalsQuiz.Questions[0];
        var noviSad = question.AnswerOptions[1];

        var response = await client.DeleteAsync(
            $"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}/questions/{question.Id}/answer-options/{noviSad.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = LoadQuiz(assertContext, QuizSeed.CapitalsQuiz.Id);
        stored.Questions[0].AnswerOptions.Should().NotContain(option => option.Id == noviSad.Id);
        stored.Questions[0].AnswerOptions.Should().ContainSingle();
    }

    [Fact]
    public async Task Administrator_marks_an_answer_option_as_the_correct_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var question = QuizSeed.CapitalsQuiz.Questions[0];
        var belgrade = question.AnswerOptions[0];
        var noviSad = question.AnswerOptions[1];

        var response = await client.PostAsync(
            $"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}/questions/{question.Id}/answer-options/{noviSad.Id}/mark-correct", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = LoadQuiz(assertContext, QuizSeed.CapitalsQuiz.Id);
        stored.Questions[0].AnswerOptions.Single(option => option.Id == noviSad.Id).IsCorrect.Should().BeTrue();
        stored.Questions[0].AnswerOptions.Single(option => option.Id == belgrade.Id).IsCorrect.Should().BeFalse();
    }

    [Fact]
    public async Task Administrator_cannot_add_a_question_to_a_quiz_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var missingQuizId = Guid.NewGuid();
        var request = new TextDto("Glavni grad Francuske?");

        var response = await client.PostAsJsonAsync($"{BaseUrl}/{missingQuizId}/questions", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == missingQuizId);
    }

    [Theory]
    [MemberData(nameof(Commands))]
    public async Task Anonymous_user_cannot_compose_a_quiz(string method, string url)
    {
        var client = Factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(Commands))]
    public async Task Explorer_cannot_compose_a_quiz(string method, string url)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
