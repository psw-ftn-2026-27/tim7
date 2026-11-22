using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz DraftWithEmptyQuestion;
    public static readonly Quiz CapitalsQuiz;
    public static readonly Quiz PublishedRiversQuiz;
    public static readonly Quiz RetiredBridgesQuiz;
    public static readonly Quiz ArchivedMonumentsQuiz;

    static QuizSeed()
    {
        DraftWithEmptyQuestion = new Quiz("Nedovršen kviz", null, null);
        DraftWithEmptyQuestion.AddQuestion("Pitanje bez odgovora?");

        CapitalsQuiz = new Quiz("Prestonice Evrope", "Izaberi tačan odgovor.", 50);
        CapitalsQuiz.AddQuestion("Glavni grad Srbije?");
        CapitalsQuiz.AddAnswerOption(CapitalsQuiz.Questions[0].Id, "Beograd");
        CapitalsQuiz.AddAnswerOption(CapitalsQuiz.Questions[0].Id, "Novi Sad");
        CapitalsQuiz.MarkAnswerOptionCorrect(CapitalsQuiz.Questions[0].Id, CapitalsQuiz.Questions[0].AnswerOptions[0].Id);

        PublishedRiversQuiz = new Quiz("Reke Srbije", "Izaberi tačan odgovor.", 40);
        PublishedRiversQuiz.AddQuestion("Koja reka protiče kroz Novi Sad?");
        PublishedRiversQuiz.AddAnswerOption(PublishedRiversQuiz.Questions[0].Id, "Dunav");
        PublishedRiversQuiz.AddAnswerOption(PublishedRiversQuiz.Questions[0].Id, "Sava");
        PublishedRiversQuiz.MarkAnswerOptionCorrect(PublishedRiversQuiz.Questions[0].Id, PublishedRiversQuiz.Questions[0].AnswerOptions[0].Id);
        PublishedRiversQuiz.Publish();

        RetiredBridgesQuiz = new Quiz("Mostovi Beograda", "Izaberi tačan odgovor.", 30);
        RetiredBridgesQuiz.AddQuestion("Koji most je najstariji?");
        RetiredBridgesQuiz.AddAnswerOption(RetiredBridgesQuiz.Questions[0].Id, "Stari savski most");
        RetiredBridgesQuiz.AddAnswerOption(RetiredBridgesQuiz.Questions[0].Id, "Most na Adi");
        RetiredBridgesQuiz.MarkAnswerOptionCorrect(RetiredBridgesQuiz.Questions[0].Id, RetiredBridgesQuiz.Questions[0].AnswerOptions[0].Id);
        RetiredBridgesQuiz.Publish();
        RetiredBridgesQuiz.Retire();

        ArchivedMonumentsQuiz = new Quiz("Spomenici Novog Sada", "Izaberi tačan odgovor.", 60);
        ArchivedMonumentsQuiz.AddQuestion("Čiji spomenik stoji na Trgu slobode?");
        ArchivedMonumentsQuiz.AddAnswerOption(ArchivedMonumentsQuiz.Questions[0].Id, "Svetozara Miletića");
        ArchivedMonumentsQuiz.AddAnswerOption(ArchivedMonumentsQuiz.Questions[0].Id, "Jovana Jovanovića Zmaja");
        ArchivedMonumentsQuiz.MarkAnswerOptionCorrect(ArchivedMonumentsQuiz.Questions[0].Id, ArchivedMonumentsQuiz.Questions[0].AnswerOptions[0].Id);
        ArchivedMonumentsQuiz.Publish();
        ArchivedMonumentsQuiz.Retire();
        ArchivedMonumentsQuiz.Archive();
    }

    public static Quiz[] All => [DraftWithEmptyQuestion, CapitalsQuiz, PublishedRiversQuiz, RetiredBridgesQuiz, ArchivedMonumentsQuiz];
}
