using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application.BlogReading;

public sealed class BlogReadingQueries
{
    private readonly IBlogReadRepository _blogReadRepository;
    private readonly IIdentityApi _identityApi;

    public BlogReadingQueries(IBlogReadRepository blogReadRepository, IIdentityApi identityApi)
    {
        _blogReadRepository = blogReadRepository;
        _identityApi = identityApi;
    }

    public async Task<BlogDto> GetByIdAsync(Guid id, Guid readerId)
    {
        var blog = await _blogReadRepository.GetByIdAsync(id, readerId);
        if (blog is null || (blog.Status != BlogStatus.Published && blog.Status != BlogStatus.Archived))
        {
            throw new NotFoundException($"Blog {id} does not exist.");
        }

        var commenterIds = blog.Comments.Select(comment => comment.UserId).Distinct().ToList();
        var commenters = await _identityApi.GetUsersByIdsAsync(commenterIds);
        var comments = blog.Comments
            .Select(comment => comment with
            {
                UserEmail = commenters.FirstOrDefault(user => user.Id == comment.UserId)?.Email
            })
            .ToList();

        return blog with { Comments = comments };
    }

    public Task<PageResult<BlogDto>> GetVisibleAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _blogReadRepository.GetVisibleAsync(page, pageSize);
    }
}
