using Shared.Domain.Chats;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class ChatSeed
{
    public static readonly Chat VineyardsChat;
    public static readonly Message VineyardsVisitorQuestion;
    public static readonly Chat ArchivedLakesideChat;

    static ChatSeed()
    {
        var vineyards = TourSeed.PublishedVineyards;
        VineyardsChat = vineyards.OpenChat();
        VineyardsVisitorQuestion = VineyardsChat.Write(vineyards.ChatParticipantFor(WellKnownUsers.SecondExplorer), "Da li je tura pogodna za decu?", new DateTime(2026, 11, 16, 10, 0, 0, DateTimeKind.Utc));

        ArchivedLakesideChat = TourSeed.ArchivedLakeside.OpenChat();
    }

    public static object[] All => [VineyardsChat, ArchivedLakesideChat, VineyardsVisitorQuestion];
}
