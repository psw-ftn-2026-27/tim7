using FluentAssertions;
using Shared.Domain.Chats;
using Shared.Domain.Exceptions;
using Xunit;

namespace Shared.UnitTests.Chats;

public class ChatTests
{
    private static Chat OpenChat()
    {
        return Chat.Open(new ChatContext("club", Guid.NewGuid()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Message_with_empty_text_is_rejected(string text)
    {
        var chat = OpenChat();

        var writing = () => chat.Write(new ChatParticipant(Guid.NewGuid(), false), text, DateTime.UtcNow);

        writing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Message_longer_than_1000_characters_is_rejected()
    {
        var chat = OpenChat();

        var writing = () => chat.Write(new ChatParticipant(Guid.NewGuid(), false), new string('a', 1001), DateTime.UtcNow);

        writing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Message_of_exactly_1000_characters_is_accepted()
    {
        var chat = OpenChat();

        var message = chat.Write(new ChatParticipant(Guid.NewGuid(), false), new string('a', 1000), DateTime.UtcNow);

        message.Text.Should().HaveLength(1000);
    }

    [Fact]
    public void Locked_chat_rejects_writing()
    {
        var chat = OpenChat();
        chat.Lock();

        var writing = () => chat.Write(new ChatParticipant(Guid.NewGuid(), true), "Hello", DateTime.UtcNow);

        writing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Writing_into_an_open_chat_gives_a_message_of_that_chat_from_that_participant()
    {
        var chat = OpenChat();
        var participant = new ChatParticipant(Guid.NewGuid(), false);

        var message = chat.Write(participant, "Hello", DateTime.UtcNow);

        message.ChatId.Should().Be(chat.Id);
        message.AuthorId.Should().Be(participant.UserId);
    }
}
