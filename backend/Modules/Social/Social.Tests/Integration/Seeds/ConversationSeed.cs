using Shared.Domain.Chats;
using Shared.Tests;
using Social.Domain.Conversations;

namespace Social.Tests.Integration.Seeds;

internal static class ConversationSeed
{
    public static readonly Conversation DemoExplorersConversation;
    public static readonly Chat DemoExplorersChat;
    public static readonly Message DemoExplorerGreeting;

    static ConversationSeed()
    {
        DemoExplorersConversation = Conversation.Start(DemoUsers.Explorer1, DemoUsers.Explorer2, true);
        DemoExplorersChat = Chat.Open(DemoExplorersConversation.ChatContext);
        DemoExplorerGreeting = DemoExplorersChat.Write(DemoExplorersConversation.ChatParticipantFor(DemoUsers.Explorer1), "Ćao, ideš li na turu u subotu?", new DateTime(2026, 11, 18, 9, 0, 0, DateTimeKind.Utc));
    }

    public static object[] All => [DemoExplorersConversation, DemoExplorersChat, DemoExplorerGreeting];
}
