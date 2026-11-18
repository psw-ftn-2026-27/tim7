using Shared.Domain.Chats;
using Shared.Tests;

namespace Social.Tests.Integration.Seeds;

internal static class ChatSeed
{
    public static readonly Chat WalkersChat;
    public static readonly Message WalkersDemoMemberGreeting;
    public static readonly Message WalkersDeletedMessage;

    public static readonly Chat SwimmersChatWithMutedExplorer;
    public static readonly Message SwimmersOwnerMessage;

    static ChatSeed()
    {
        var walkers = ClubSeed.WalkersWithDemoMember;
        WalkersChat = Chat.Open(walkers.ChatContext);
        WalkersDemoMemberGreeting = WalkersChat.Write(walkers.ChatParticipantFor(DemoUsers.Explorer1), "Vidimo se večeras na tvrđavi.", new DateTime(2026, 11, 16, 18, 0, 0, DateTimeKind.Utc));
        WalkersDeletedMessage = WalkersChat.Write(walkers.ChatParticipantFor(DemoUsers.Explorer1), "Neprimerena poruka.", new DateTime(2026, 11, 16, 18, 5, 0, DateTimeKind.Utc));
        WalkersDeletedMessage.DeleteBy(walkers.ChatParticipantFor(WellKnownUsers.Explorer));

        var swimmers = ClubSeed.SwimmersWithExplorerMemberAndPendingRequest;
        SwimmersChatWithMutedExplorer = Chat.Open(swimmers.ChatContext);
        SwimmersOwnerMessage = SwimmersChatWithMutedExplorer.Write(swimmers.ChatParticipantFor(WellKnownUsers.SecondExplorer), "Sutra plivamo u sedam.", new DateTime(2026, 11, 17, 7, 0, 0, DateTimeKind.Utc));
        SwimmersChatWithMutedExplorer.Mute(swimmers.ChatParticipantFor(WellKnownUsers.SecondExplorer), swimmers.ChatParticipantFor(WellKnownUsers.Explorer));
    }

    public static object[] All => [WalkersChat, SwimmersChatWithMutedExplorer, WalkersDemoMemberGreeting, WalkersDeletedMessage, SwimmersOwnerMessage];
}
