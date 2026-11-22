using System.Net;
using FluentAssertions;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizLifecycle;

public class QuizLifecycleCommandTests : BaseIntegrationTest
{
    private const string BaseUrl = "/api/games/quizzes";

    public QuizLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    public static IEnumerable<object[]> Commands()
    {
        return CommandsFor(QuizSeed.CapitalsQuiz.Id);
    }

    public static IEnumerable<object[]> CommandsForMissingQuiz()
    {
        return CommandsFor(Guid.NewGuid());
    }

    private static IEnumerable<object[]> CommandsFor(Guid quizId)
    {
        var quizUrl = $"{BaseUrl}/{quizId}";

        yield return [HttpMethod.Post.Method, $"{quizUrl}/publish"];
        yield return [HttpMethod.Post.Method, $"{quizUrl}/retire"];
        yield return [HttpMethod.Post.Method, $"{quizUrl}/restore"];
        yield return [HttpMethod.Post.Method, $"{quizUrl}/archive"];
        yield return [HttpMethod.Delete.Method, quizUrl];
    }

    private QuizStatus StoredStatus(Guid quizId)
    {
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        return assertContext.Quizzes.Single(quiz => quiz.Id == quizId).Status;
    }

    [Fact]
    public async Task Administrator_publishes_a_complete_draft_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredStatus(QuizSeed.CapitalsQuiz.Id).Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Administrator_retires_a_published_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{BaseUrl}/{QuizSeed.PublishedRiversQuiz.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredStatus(QuizSeed.PublishedRiversQuiz.Id).Should().Be(QuizStatus.Retired);
    }

    [Fact]
    public async Task Administrator_restores_a_retired_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{BaseUrl}/{QuizSeed.RetiredBridgesQuiz.Id}/restore", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredStatus(QuizSeed.RetiredBridgesQuiz.Id).Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{BaseUrl}/{QuizSeed.RetiredBridgesQuiz.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredStatus(QuizSeed.RetiredBridgesQuiz.Id).Should().Be(QuizStatus.Archived);
    }

    [Fact]
    public async Task Administrator_deletes_an_archived_quiz_with_its_questions_and_answer_options()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = QuizSeed.ArchivedMonumentsQuiz;
        var questionIds = quiz.Questions.Select(question => question.Id).ToList();
        var answerOptionIds = quiz.Questions.SelectMany(question => question.AnswerOptions).Select(option => option.Id).ToList();

        var response = await client.DeleteAsync($"{BaseUrl}/{quiz.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Should().NotContain(stored => stored.Id == quiz.Id);
        assertContext.Set<Question>().Should().NotContain(question => questionIds.Contains(question.Id));
        assertContext.Set<AnswerOption>().Should().NotContain(option => answerOptionIds.Contains(option.Id));
    }

    [Fact]
    public async Task Administrator_cannot_publish_an_incomplete_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"{BaseUrl}/{QuizSeed.DraftWithEmptyQuestion.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        StoredStatus(QuizSeed.DraftWithEmptyQuestion.Id).Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [MemberData(nameof(CommandsForMissingQuiz))]
    public async Task Administrator_cannot_change_the_lifecycle_of_a_quiz_that_does_not_exist(string method, string url)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
    }

    [Theory]
    [MemberData(nameof(Commands))]
    public async Task Anonymous_user_cannot_change_the_lifecycle_of_a_quiz(string method, string url)
    {
        var client = Factory.CreateClient();

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(Commands))]
    public async Task Explorer_cannot_change_the_lifecycle_of_a_quiz(string method, string url)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
