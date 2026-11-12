using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club FruskaGoraHikers = new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend ture po stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"]);
    public static readonly Club OldTownPhotographers = new(WellKnownUsers.Explorer, "Fotografi starog grada", "Šetnje sa fotoaparatom kroz stari deo grada.", ["https://example.com/stari-grad.jpg", "https://example.com/katedrala.jpg"]);
    public static readonly Club SecondExplorersCyclists = new(WellKnownUsers.SecondExplorer, "Biciklisti uz Dunav", "Vožnje biciklističkom stazom uz Dunav.", ["https://example.com/dunav.jpg"]);

    public static readonly Club BirdWatchers = new(WellKnownUsers.Explorer, "Posmatrači ptica", "Jutarnje posmatranje ptica na Koviljskom ritu.", ["https://example.com/kovilj.jpg"]);
    public static readonly Club SecondExplorersBirdWatchers = new(WellKnownUsers.SecondExplorer, "Posmatrači ptica", "Posmatranje ptica na Carskoj bari.", ["https://example.com/carska-bara.jpg"]);

    public static Club[] All => [FruskaGoraHikers, OldTownPhotographers, SecondExplorersCyclists, BirdWatchers, SecondExplorersBirdWatchers];
}
