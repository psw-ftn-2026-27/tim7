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

    public static readonly Club RunnersWithMemberAndPendingRequest;
    public static readonly Club SwimmersWithExplorerMemberAndPendingRequest;
    public static readonly Club ClimbersWithExplorerModeratorAndPendingRequest;

    static ClubSeed()
    {
        RunnersWithMemberAndPendingRequest = new(WellKnownUsers.Explorer, "Trkači Dunavskog parka", "Jutarnje trčanje kroz Dunavski park.", ["https://example.com/dunavski-park.jpg"]);
        RunnersWithMemberAndPendingRequest.RequestToJoin(WellKnownUsers.Administrator);
        RunnersWithMemberAndPendingRequest.AcceptRequest(WellKnownUsers.Explorer, WellKnownUsers.Administrator);
        RunnersWithMemberAndPendingRequest.RequestToJoin(WellKnownUsers.SecondExplorer);

        SwimmersWithExplorerMemberAndPendingRequest = new(WellKnownUsers.SecondExplorer, "Plivači Štranda", "Jutarnje plivanje na Štrandu.", ["https://example.com/strand.jpg"]);
        SwimmersWithExplorerMemberAndPendingRequest.RequestToJoin(WellKnownUsers.Explorer);
        SwimmersWithExplorerMemberAndPendingRequest.AcceptRequest(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);
        SwimmersWithExplorerMemberAndPendingRequest.RequestToJoin(WellKnownUsers.Administrator);

        ClimbersWithExplorerModeratorAndPendingRequest = new(WellKnownUsers.SecondExplorer, "Penjači Kamenice", "Penjanje po stenama kod Kamenice.", ["https://example.com/kamenica.jpg"]);
        ClimbersWithExplorerModeratorAndPendingRequest.RequestToJoin(WellKnownUsers.Explorer);
        ClimbersWithExplorerModeratorAndPendingRequest.AcceptRequest(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);
        ClimbersWithExplorerModeratorAndPendingRequest.PromoteToModerator(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);
        ClimbersWithExplorerModeratorAndPendingRequest.RequestToJoin(WellKnownUsers.Administrator);
    }

    public static Club[] All => [FruskaGoraHikers, OldTownPhotographers, SecondExplorersCyclists, BirdWatchers, SecondExplorersBirdWatchers, RunnersWithMemberAndPendingRequest, SwimmersWithExplorerMemberAndPendingRequest, ClimbersWithExplorerModeratorAndPendingRequest];
}
