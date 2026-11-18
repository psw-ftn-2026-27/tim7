using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz DraftWithEmptyQuestion;
    public static readonly Quiz CapitalsQuiz;

    static QuizSeed()
    {
        DraftWithEmptyQuestion = new Quiz("Nedovršen kviz", null, null);
        DraftWithEmptyQuestion.AddQuestion("Pitanje bez odgovora?");

        CapitalsQuiz = new Quiz("Prestonice Evrope", "Izaberi tačan odgovor.", 50);
        CapitalsQuiz.AddQuestion("Glavni grad Srbije?");
        CapitalsQuiz.AddAnswerOption(CapitalsQuiz.Questions[0].Id, "Beograd");
        CapitalsQuiz.AddAnswerOption(CapitalsQuiz.Questions[0].Id, "Novi Sad");
        CapitalsQuiz.MarkAnswerOptionCorrect(CapitalsQuiz.Questions[0].Id, CapitalsQuiz.Questions[0].AnswerOptions[0].Id);
    }

    public static Quiz[] All => [DraftWithEmptyQuestion, CapitalsQuiz];
}
