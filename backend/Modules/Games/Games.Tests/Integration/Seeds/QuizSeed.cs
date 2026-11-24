using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz DraftWithEmptyQuestion;
    public static readonly Quiz CapitalsQuiz;
    public static readonly Quiz PublishedRiversQuiz;
    public static readonly Quiz RetiredBridgesQuiz;
    public static readonly Quiz ArchivedMonumentsQuiz;
    public static readonly Quiz PublishedFortressesQuiz;

    static QuizSeed()
    {
        DraftWithEmptyQuestion = new Quiz("Nedovršen kviz", null, null);
        DraftWithEmptyQuestion.AddQuestion("Pitanje bez odgovora?");

        CapitalsQuiz = new Quiz("Prestonice Evrope", "Izaberi tačan odgovor.", 50);
        CapitalsQuiz.AddQuestion("Glavni grad Srbije?");
        CapitalsQuiz.AddAnswerOption(CapitalsQuiz.Questions[0].Id, "Beograd");
        CapitalsQuiz.AddAnswerOption(CapitalsQuiz.Questions[0].Id, "Novi Sad");
        CapitalsQuiz.MarkAnswerOptionCorrect(CapitalsQuiz.Questions[0].Id, CapitalsQuiz.Questions[0].AnswerOptions[0].Id);
        CapitalsQuiz.SetLocation(new Location(44.8176, 20.4569));

        PublishedRiversQuiz = new Quiz("Reke Srbije", "Izaberi tačan odgovor.", 40);
        PublishedRiversQuiz.AddQuestion("Koja reka protiče kroz Novi Sad?");
        PublishedRiversQuiz.AddAnswerOption(PublishedRiversQuiz.Questions[0].Id, "Dunav");
        PublishedRiversQuiz.AddAnswerOption(PublishedRiversQuiz.Questions[0].Id, "Sava");
        PublishedRiversQuiz.MarkAnswerOptionCorrect(PublishedRiversQuiz.Questions[0].Id, PublishedRiversQuiz.Questions[0].AnswerOptions[0].Id);
        PublishedRiversQuiz.SetLocation(new Location(45.2551, 19.8452));
        PublishedRiversQuiz.Publish();

        RetiredBridgesQuiz = new Quiz("Mostovi Beograda", "Izaberi tačan odgovor.", 30);
        RetiredBridgesQuiz.AddQuestion("Koji most je najstariji?");
        RetiredBridgesQuiz.AddAnswerOption(RetiredBridgesQuiz.Questions[0].Id, "Stari savski most");
        RetiredBridgesQuiz.AddAnswerOption(RetiredBridgesQuiz.Questions[0].Id, "Most na Adi");
        RetiredBridgesQuiz.MarkAnswerOptionCorrect(RetiredBridgesQuiz.Questions[0].Id, RetiredBridgesQuiz.Questions[0].AnswerOptions[0].Id);
        RetiredBridgesQuiz.SetLocation(new Location(44.8125, 20.4500));
        RetiredBridgesQuiz.Publish();
        RetiredBridgesQuiz.Retire();

        ArchivedMonumentsQuiz = new Quiz("Spomenici Novog Sada", "Izaberi tačan odgovor.", 60);
        ArchivedMonumentsQuiz.AddQuestion("Čiji spomenik stoji na Trgu slobode?");
        ArchivedMonumentsQuiz.AddAnswerOption(ArchivedMonumentsQuiz.Questions[0].Id, "Svetozara Miletića");
        ArchivedMonumentsQuiz.AddAnswerOption(ArchivedMonumentsQuiz.Questions[0].Id, "Jovana Jovanovića Zmaja");
        ArchivedMonumentsQuiz.MarkAnswerOptionCorrect(ArchivedMonumentsQuiz.Questions[0].Id, ArchivedMonumentsQuiz.Questions[0].AnswerOptions[0].Id);
        ArchivedMonumentsQuiz.SetLocation(new Location(45.2550, 19.8450));
        ArchivedMonumentsQuiz.Publish();
        ArchivedMonumentsQuiz.Retire();
        ArchivedMonumentsQuiz.Archive();

        PublishedFortressesQuiz = new Quiz("Tvrđave Srbije", "Izaberi tačan odgovor.", 50);
        PublishedFortressesQuiz.AddQuestion("Na kojoj reci je Petrovaradinska tvrđava?");
        PublishedFortressesQuiz.AddAnswerOption(PublishedFortressesQuiz.Questions[0].Id, "Dunav");
        PublishedFortressesQuiz.AddAnswerOption(PublishedFortressesQuiz.Questions[0].Id, "Tisa");
        PublishedFortressesQuiz.MarkAnswerOptionCorrect(PublishedFortressesQuiz.Questions[0].Id, PublishedFortressesQuiz.Questions[0].AnswerOptions[0].Id);
        PublishedFortressesQuiz.AddQuestion("U kom gradu je Kalemegdan?");
        PublishedFortressesQuiz.AddAnswerOption(PublishedFortressesQuiz.Questions[1].Id, "Beograd");
        PublishedFortressesQuiz.AddAnswerOption(PublishedFortressesQuiz.Questions[1].Id, "Niš");
        PublishedFortressesQuiz.MarkAnswerOptionCorrect(PublishedFortressesQuiz.Questions[1].Id, PublishedFortressesQuiz.Questions[1].AnswerOptions[0].Id);
        PublishedFortressesQuiz.SetLocation(new Location(45.2517, 19.8622));
        PublishedFortressesQuiz.Publish();
    }

    public static Quiz[] All => [DraftWithEmptyQuestion, CapitalsQuiz, PublishedRiversQuiz, RetiredBridgesQuiz, ArchivedMonumentsQuiz, PublishedFortressesQuiz];
}
