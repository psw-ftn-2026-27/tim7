using FluentAssertions;
using Shared.Domain.Chats;
using Shared.Domain.Exceptions;
using Xunit;

namespace Shared.UnitTests.Chats;

public class ChatModerationTests
{
    private static readonly Guid ModeratorId = Guid.NewGuid();
    private static readonly Guid OtherModeratorId = Guid.NewGuid();
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly Guid OtherMemberId = Guid.NewGuid();

    private static readonly ChatParticipant Moderator = new(ModeratorId, true);
    private static readonly ChatParticipant OtherModerator = new(OtherModeratorId, true);
    private static readonly ChatParticipant Member = new(MemberId, false);
    private static readonly ChatParticipant OtherMember = new(OtherMemberId, false);

    private static Chat OpenChat()
    {
        return Chat.Open(new ChatContext("club", Guid.NewGuid()));
    }

    public static TheoryData<ChatParticipant, ChatParticipant> RejectedMutes => new()
    {
        { Member, OtherMember },
        { Moderator, OtherModerator },
        { Moderator, Moderator },
    };

    [Theory]
    [MemberData(nameof(RejectedMutes))]
    public void Mute_is_rejected_for_an_unprivileged_moderator_a_privileged_target_or_oneself(
        ChatParticipant by, ChatParticipant target)
    {
        var chat = OpenChat();

        var muting = () => chat.Mute(by, target);

        muting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Unprivileged_participant_cannot_lift_a_mute()
    {
        var chat = OpenChat();
        chat.Mute(Moderator, Member);

        var unmuting = () => chat.Unmute(Member, MemberId);

        unmuting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Muted_unprivileged_participant_cannot_write()
    {
        var chat = OpenChat();
        chat.Mute(Moderator, Member);

        var writing = () => chat.Write(Member, "Hello", DateTime.UtcNow);

        writing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Muted_participant_who_became_privileged_can_write()
    {
        var chat = OpenChat();
        chat.Mute(Moderator, Member);

        var message = chat.Write(new ChatParticipant(MemberId, true), "Hello", DateTime.UtcNow);

        message.AuthorId.Should().Be(MemberId);
    }

    [Fact]
    public void Unprivileged_participant_cannot_delete_another_participants_message()
    {
        var message = OpenChat().Write(OtherMember, "Hello", DateTime.UtcNow);

        var deleting = () => message.DeleteBy(Member);

        deleting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Nobody_can_delete_their_own_message()
    {
        var message = OpenChat().Write(Moderator, "Hello", DateTime.UtcNow);

        var deleting = () => message.DeleteBy(Moderator);

        deleting.Should().Throw<DomainException>();
    }
}
