using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club FruskaGoraHikers = new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);
    public static readonly Club OldTownPhotographers = new(WellKnownUsers.Explorer, "Fotografi starog grada", "Šetnje sa fotoaparatom kroz stari deo grada.", ["https://example.com/stari-grad.jpg", "https://example.com/katedrala.jpg"]);
    public static readonly Club SecondExplorersCyclists = new(WellKnownUsers.SecondExplorer, "Biciklisti uz Dunav", "Vožnje biciklističkom stazom uz Dunav.", ["https://example.com/dunav.jpg"]);

    public static Club[] All => [FruskaGoraHikers, OldTownPhotographers, SecondExplorersCyclists];
}
