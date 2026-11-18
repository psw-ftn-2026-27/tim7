using AutoMapper;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application;

public sealed class SocialMapperProfile : Profile
{
    public SocialMapperProfile()
    {
        // The reader's vote is known only to the read side, so a mapped blog carries none.
        CreateMap<Blog, BlogDto>()
            .ForCtorParam(nameof(BlogDto.Score), options => options.MapFrom(blog =>
                blog.Votes.Count(vote => vote.Type == BlogVoteType.Upvote)
                - blog.Votes.Count(vote => vote.Type == BlogVoteType.Downvote)))
            .ForCtorParam(nameof(BlogDto.MyVote), options => options.MapFrom(blog => (BlogVoteType?)null));
        // The commenter's e-mail is known only to Identity, so a mapped comment carries none.
        CreateMap<Comment, CommentDto>()
            .ForCtorParam(nameof(CommentDto.UserEmail), options => options.MapFrom(comment => (string?)null));
    }
}
