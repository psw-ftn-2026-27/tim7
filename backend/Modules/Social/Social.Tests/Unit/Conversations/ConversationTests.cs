using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Conversations;
using Xunit;

namespace Social.Tests.Unit.Conversations;

public class ConversationTests
{
    [Fact]
    public void Conversation_cannot_be_started_with_a_user_one_does_not_follow()
    {
        var starting = () => Conversation.Start(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, false);

        starting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Conversation_cannot_be_started_with_oneself()
    {
        var starting = () => Conversation.Start(WellKnownUsers.Explorer, WellKnownUsers.Explorer, true);

        starting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Both_users_take_part_without_privileges()
    {
        var conversation = Conversation.Start(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, true);

        var participants = new[] { WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer }
            .Select(conversation.ChatParticipantFor);

        participants.Should().OnlyContain(participant => !participant.IsPrivileged);
    }

    [Fact]
    public void Nobody_else_takes_part_in_the_conversation()
    {
        var conversation = Conversation.Start(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, true);

        var joining = () => conversation.ChatParticipantFor(WellKnownUsers.Administrator);

        joining.Should().Throw<DomainException>();
    }
}
