using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Infrastructure.Persistence.Blogs;

internal sealed class BlogReadRepository : IBlogReadRepository
{
    private readonly SocialDbContext _dbContext;

    public BlogReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BlogDto?> GetByIdAsync(Guid id, Guid readerId)
    {
        return ProjectToDtos(_dbContext.Blogs.Where(blog => blog.Id == id), readerId).FirstOrDefaultAsync();
    }

    public Task<List<BlogDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Blogs.Where(blog => blog.AuthorId == authorId), null).ToListAsync();
    }

    public async Task<PageResult<BlogDto>> GetVisibleAsync(int page, int pageSize)
    {
        var visible = _dbContext.Blogs
            .Where(blog => blog.Status == BlogStatus.Published || blog.Status == BlogStatus.Archived)
            .OrderByDescending(blog => blog.CreatedAt);

        var totalCount = await visible.CountAsync();
        var items = await ProjectToDtos(visible, null)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<BlogDto>(items, totalCount);
    }

    // Without a reader, MyVote stays empty; the score is derived from the votes in every projection.
    private static IQueryable<BlogDto> ProjectToDtos(IQueryable<Blog> blogs, Guid? readerId)
    {
        return blogs
            .Select(blog => new BlogDto(
                blog.Id,
                blog.AuthorId,
                blog.Title,
                blog.Description,
                blog.CreatedAt,
                blog.Images,
                blog.Status,
                blog.LastModifiedAt,
                blog.Comments
                    .Select(comment => new CommentDto(comment.Id, comment.UserId, comment.Text, comment.CreatedAt))
                    .ToList(),
                blog.Votes.Count(vote => vote.Type == BlogVoteType.Upvote)
                    - blog.Votes.Count(vote => vote.Type == BlogVoteType.Downvote),
                blog.Votes
                    .Where(vote => vote.UserId == readerId)
                    .Select(vote => (BlogVoteType?)vote.Type)
                    .FirstOrDefault()));
    }
}
