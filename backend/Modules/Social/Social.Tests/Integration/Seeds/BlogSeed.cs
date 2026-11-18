using Shared.Tests;
using Social.Domain.Blogs;

namespace Social.Tests.Integration.Seeds;

internal static class BlogSeed
{
    public static readonly Blog FortressImpressions = new(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Obilazak Petrovaradinske tvrđave ostavio je snažan utisak: podzemne galerije, Sahat kula i pogled na Dunav.", ["https://example.com/tvrdjava.jpg"]);
    public static readonly Blog CityNotes = new(WellKnownUsers.Explorer, "Beleške iz grada", "Kratke beleške sa šetnje centrom grada.", []);
    public static readonly Blog SecondExplorersDiary = new(WellKnownUsers.SecondExplorer, "Tuđi dnevnik", "Beleške drugog autora, vidljive samo njemu.", []);
    public static readonly Blog PublishedRiverbank;
    public static readonly Blog CommentedMuseumVisit;
    public static readonly Blog ArchivedDanubeCruise;
    public static readonly Blog VotedLakeTrip;

    static BlogSeed()
    {
        PublishedRiverbank = new(WellKnownUsers.Explorer, "Zalasci na keju", "Fotografije i utisci sa večernje šetnje novosadskim kejom.", []);
        PublishedRiverbank.Publish();

        CommentedMuseumVisit = new(WellKnownUsers.Explorer, "Poseta muzeju", "Utisci sa stalne postavke Muzeja Vojvodine.", []);
        CommentedMuseumVisit.Publish();
        CommentedMuseumVisit.AddComment(WellKnownUsers.Administrator, "Odlična preporuka!");

        ArchivedDanubeCruise = new(WellKnownUsers.Explorer, "Krstarenje Dunavom", "Završen dnevnik jednodnevnog krstarenja od Novog Sada do Beočina.", []);
        ArchivedDanubeCruise.Publish();
        ArchivedDanubeCruise.Archive();

        VotedLakeTrip = new(WellKnownUsers.Explorer, "Izlet na jezero", "Utisci sa celodnevnog izleta na Palićko jezero.", []);
        VotedLakeTrip.Publish();
        VotedLakeTrip.Vote(WellKnownUsers.Administrator, BlogVoteType.Upvote);
        VotedLakeTrip.Vote(WellKnownUsers.SecondExplorer, BlogVoteType.Upvote);
        VotedLakeTrip.Vote(WellKnownUsers.Explorer, BlogVoteType.Downvote);
    }

    public static Blog[] All => [FortressImpressions, CityNotes, SecondExplorersDiary, PublishedRiverbank, CommentedMuseumVisit, ArchivedDanubeCruise, VotedLakeTrip];
}
