using Games.Domain.Quizzes;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class QuizAttemptSeed
{
    public static readonly QuizAttempt ExplorerStartedRiversQuiz;
    public static readonly QuizAttempt ExplorerHalfwayThroughFortressesQuiz;
    public static readonly QuizAttempt SecondExplorerSolvedFortressesQuiz;

    static QuizAttemptSeed()
    {
        var rivers = QuizSeed.PublishedRiversQuiz;
        var fortresses = QuizSeed.PublishedFortressesQuiz;
        var fortressQuestionIds = fortresses.Questions.Select(question => question.Id).ToList();
        var firstQuestion = fortresses.Questions[0];
        var secondQuestion = fortresses.Questions[1];

        ExplorerStartedRiversQuiz = new QuizAttempt(WellKnownUsers.Explorer, rivers.Id);

        ExplorerHalfwayThroughFortressesQuiz = new QuizAttempt(WellKnownUsers.Explorer, fortresses.Id);
        ExplorerHalfwayThroughFortressesQuiz.Answer(
            new GivenAnswer(firstQuestion.Id, firstQuestion.AnswerOptions[0].Id, fortresses.CheckAnswer(firstQuestion.Id, firstQuestion.AnswerOptions[0].Id)),
            fortressQuestionIds,
            fortresses.Xp!.Value);

        SecondExplorerSolvedFortressesQuiz = new QuizAttempt(WellKnownUsers.SecondExplorer, fortresses.Id);
        SecondExplorerSolvedFortressesQuiz.Answer(
            new GivenAnswer(firstQuestion.Id, firstQuestion.AnswerOptions[0].Id, fortresses.CheckAnswer(firstQuestion.Id, firstQuestion.AnswerOptions[0].Id)),
            fortressQuestionIds,
            fortresses.Xp!.Value);
        SecondExplorerSolvedFortressesQuiz.Answer(
            new GivenAnswer(secondQuestion.Id, secondQuestion.AnswerOptions[1].Id, fortresses.CheckAnswer(secondQuestion.Id, secondQuestion.AnswerOptions[1].Id)),
            fortressQuestionIds,
            fortresses.Xp!.Value);
    }

    public static QuizAttempt[] All => [ExplorerStartedRiversQuiz, ExplorerHalfwayThroughFortressesQuiz, SecondExplorerSolvedFortressesQuiz];
}
