using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Blogs;
using Xunit;

namespace Social.Tests.Unit.Blogs;

public class BlogTests
{
    private static Blog CreateBlog() =>
        new(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", ["https://example.com/tvrdjava.jpg"]);

    private static Blog CreatePublishedBlog()
    {
        var blog = CreateBlog();
        blog.Publish();
        return blog;
    }

    private static Blog CreateArchivedBlog()
    {
        var blog = CreatePublishedBlog();
        blog.Archive();
        return blog;
    }

    [Fact]
    public void New_blog_starts_as_a_draft()
    {
        var blog = CreateBlog();

        blog.Status.Should().Be(BlogStatus.Draft);
        blog.AuthorId.Should().Be(WellKnownUsers.Explorer);
        blog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public void New_blog_without_images_has_no_images()
    {
        var blog = new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", null);

        blog.Images.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_title(string title)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, title, "Opis obilaska tvrđave.", []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_description(string description)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", description, []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_blog_is_published()
    {
        var blog = CreateBlog();

        blog.Publish();

        blog.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public void Published_blog_cannot_be_published_again()
    {
        var blog = CreatePublishedBlog();

        var publishing = () => blog.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_blog_is_archived()
    {
        var blog = CreatePublishedBlog();

        blog.Archive();

        blog.Status.Should().Be(BlogStatus.Archived);
    }

    [Fact]
    public void Draft_blog_cannot_be_archived()
    {
        var blog = CreateBlog();

        var archiving = () => blog.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_edited()
    {
        var blog = CreateArchivedBlog();

        var edit = () => blog.Edit("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", [], []);

        edit.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_commented_on()
    {
        var blog = CreateArchivedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_published_again()
    {
        var blog = CreateArchivedBlog();

        var publishing = () => blog.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_comments_on_a_published_blog()
    {
        var blog = CreatePublishedBlog();

        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        blog.Comments.Should().ContainSingle(comment =>
            comment.UserId == WellKnownUsers.Administrator && comment.Text == "Odlična tura!");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Comment_requires_text(string text)
    {
        var blog = CreatePublishedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, text);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_edits_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.", comment.CreatedAt.AddMinutes(5));

        blog.Comments.Single().Text.Should().Be("Ipak prosečna tura.");
    }

    [Fact]
    public void User_cannot_edit_another_users_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Explorer, "Izmena tuđeg komentara.", comment.CreatedAt.AddMinutes(5));

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_deletes_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.DeleteComment(comment.Id, WellKnownUsers.Administrator, comment.CreatedAt.AddMinutes(5));

        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public void Comment_edited_within_fifteen_minutes_has_the_new_text_and_the_time_of_the_edit()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();
        var editedAt = comment.CreatedAt.AddMinutes(14);

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.", editedAt);

        blog.Comments.Single().Text.Should().Be("Ipak prosečna tura.");
        blog.Comments.Single().LastModifiedAt.Should().Be(editedAt);
    }

    [Fact]
    public void Comment_older_than_fifteen_minutes_cannot_be_edited_and_keeps_its_text()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.", comment.CreatedAt.AddMinutes(16));

        update.Should().Throw<DomainException>();
        blog.Comments.Single().Text.Should().Be("Odlična tura!");
        blog.Comments.Single().LastModifiedAt.Should().BeNull();
    }

    [Fact]
    public void Comment_older_than_fifteen_minutes_cannot_be_deleted()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var deletion = () => blog.DeleteComment(comment.Id, WellKnownUsers.Administrator, comment.CreatedAt.AddMinutes(16));

        deletion.Should().Throw<DomainException>();
        blog.Comments.Should().ContainSingle(remaining => remaining.Id == comment.Id);
    }

    [Fact]
    public void Edit_does_not_extend_the_window_so_a_comment_edited_in_the_tenth_minute_cannot_be_edited_in_the_sixteenth()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();
        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.", comment.CreatedAt.AddMinutes(10));

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Treća verzija.", comment.CreatedAt.AddMinutes(16));

        update.Should().Throw<DomainException>();
        blog.Comments.Single().Text.Should().Be("Ipak prosečna tura.");
    }

    [Fact]
    public void Edited_blog_has_new_content_and_a_last_modification_date()
    {
        var blog = CreatePublishedBlog();

        blog.Edit("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/tvrdjava.jpg"], ["/images/social/nova.jpg"]);

        blog.Title.Should().Be("Novi utisci sa tvrđave");
        blog.Description.Should().Be("Dopunjen opis obilaska.");
        blog.Images.Should().Equal("https://example.com/tvrdjava.jpg", "/images/social/nova.jpg");
        blog.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Edited_blog_drops_the_images_it_does_not_keep()
    {
        var blog = CreateBlog();

        blog.Edit("Utisci sa tvrđave", "Opis obilaska tvrđave.", [], []);

        blog.Images.Should().BeEmpty();
    }

    [Fact]
    public void Edited_blog_cannot_keep_an_image_it_does_not_have()
    {
        var blog = CreateBlog();

        var edit = () => blog.Edit("Utisci sa tvrđave", "Opis obilaska tvrđave.", ["https://example.com/tudja.jpg"], []);

        edit.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Dopunjen opis obilaska.")]
    [InlineData("   ", "Dopunjen opis obilaska.")]
    [InlineData("Novi utisci sa tvrđave", "")]
    [InlineData("Novi utisci sa tvrđave", "   ")]
    public void Edited_blog_requires_a_title_and_a_description(string title, string description)
    {
        var blog = CreateBlog();

        var edit = () => blog.Edit(title, description, [], []);

        edit.Should().Throw<DomainException>();
    }

    [Fact]
    public void First_vote_of_a_user_is_recorded_with_its_type_user_and_time()
    {
        var blog = CreatePublishedBlog();

        blog.Vote(WellKnownUsers.Administrator, BlogVoteType.Upvote);

        var vote = blog.Votes.Should().ContainSingle().Subject;
        vote.UserId.Should().Be(WellKnownUsers.Administrator);
        vote.Type.Should().Be(BlogVoteType.Upvote);
        vote.CastAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Repeated_click_on_the_same_vote_withdraws_it()
    {
        var blog = CreatePublishedBlog();
        blog.Vote(WellKnownUsers.Administrator, BlogVoteType.Upvote);

        blog.Vote(WellKnownUsers.Administrator, BlogVoteType.Upvote);

        blog.Votes.Should().NotContain(vote => vote.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public void Click_on_the_opposite_vote_replaces_the_old_vote_with_a_new_one()
    {
        var blog = CreatePublishedBlog();
        blog.Vote(WellKnownUsers.Administrator, BlogVoteType.Upvote);
        var changedAt = DateTime.UtcNow;

        blog.Vote(WellKnownUsers.Administrator, BlogVoteType.Downvote);

        var vote = blog.Votes.Should().ContainSingle(vote => vote.UserId == WellKnownUsers.Administrator).Subject;
        vote.Type.Should().Be(BlogVoteType.Downvote);
        vote.CastAt.Should().BeOnOrAfter(changedAt);
    }

    [Theory]
    [InlineData(BlogStatus.Draft)]
    [InlineData(BlogStatus.Archived)]
    public void Blog_that_is_not_published_cannot_receive_a_vote(BlogStatus status)
    {
        var blog = status == BlogStatus.Draft ? CreateBlog() : CreateArchivedBlog();

        var voting = () => blog.Vote(WellKnownUsers.Administrator, BlogVoteType.Upvote);

        voting.Should().Throw<DomainException>();
    }
}
