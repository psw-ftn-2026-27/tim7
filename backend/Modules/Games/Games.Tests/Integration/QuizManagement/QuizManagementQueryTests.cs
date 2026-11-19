using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallenges;
using Games.Application.QuizManagement;
using Games.Domain.Quizzes;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizManagement;

public class QuizManagementQueryTests : BaseIntegrationTest
{
    private const string BaseUrl = "/api/games/quizzes";

    public QuizManagementQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_lists_all_challenges_from_the_newest_to_the_oldest()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var expectedIds = QuizSeed.All.OrderByDescending(quiz => quiz.CreatedAt).Select(quiz => quiz.Id).ToList();

        var page = await client.GetFromJsonAsync<PageResult<ChallengeListItemDto>>($"{BaseUrl}?page=1&pageSize=100", JsonOptions);

        page!.Items.Select(item => item.Id).Should().ContainInConsecutiveOrder(expectedIds);
        page.Items.Should().OnlyContain(item => item.Kind == "quiz");
        var capitals = page.Items.Single(item => item.Id == QuizSeed.CapitalsQuiz.Id);
        capitals.Name.Should().Be(QuizSeed.CapitalsQuiz.Name);
        capitals.Status.Should().Be(QuizStatus.Draft);
        capitals.Xp.Should().Be(QuizSeed.CapitalsQuiz.Xp);
        capitals.QuestionCount.Should().Be(QuizSeed.CapitalsQuiz.Questions.Count);
    }

    [Fact]
    public async Task Administrator_reads_a_quiz_with_its_questions_and_the_correct_answer_option()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seededQuestion = QuizSeed.CapitalsQuiz.Questions[0];

        var quiz = await client.GetFromJsonAsync<QuizDto>($"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}", JsonOptions);

        quiz!.Name.Should().Be(QuizSeed.CapitalsQuiz.Name);
        quiz.Instructions.Should().Be(QuizSeed.CapitalsQuiz.Instructions);
        quiz.Status.Should().Be(QuizStatus.Draft);
        quiz.Location.Should().Be(new LocationDto(QuizSeed.CapitalsQuiz.Location!.Latitude, QuizSeed.CapitalsQuiz.Location.Longitude));
        var question = quiz.Questions.Should().ContainSingle(item => item.Id == seededQuestion.Id).Subject;
        question.AnswerOptions.Should().HaveCount(seededQuestion.AnswerOptions.Count);
        question.AnswerOptions.Should().ContainSingle(option => option.IsCorrect)
            .Which.Id.Should().Be(seededQuestion.AnswerOptions[0].Id);
    }

    [Fact]
    public async Task Administrator_cannot_read_a_quiz_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"{BaseUrl}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Explorer_cannot_list_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_challenges()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_read_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonymous_user_cannot_read_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"{BaseUrl}/{QuizSeed.CapitalsQuiz.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
