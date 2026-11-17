using AutoMapper;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application.BlogAuthoring;

public sealed class BlogAuthoringService
{
    private readonly IBlogRepository _blogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStore _imageStore;
    private readonly IMapper _mapper;

    public BlogAuthoringService(IBlogRepository blogRepository, IUnitOfWork unitOfWork, IImageStore imageStore, IMapper mapper)
    {
        _blogRepository = blogRepository;
        _unitOfWork = unitOfWork;
        _imageStore = imageStore;
        _mapper = mapper;
    }

    public async Task<BlogDto> CreateAsync(Guid authorId, CreateBlogDto dto)
    {
        var images = await SaveImagesAsync(dto.Images);
        var blog = new Blog(authorId, dto.Title, dto.Description, images);

        _blogRepository.Add(blog);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BlogDto>(blog);
    }

    public async Task PublishAsync(Guid blogId, Guid authorId)
    {
        var blog = await GetOwnedBlogAsync(blogId, authorId);

        blog.Publish();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task EditAsync(Guid blogId, Guid authorId, EditBlogDto dto)
    {
        var blog = await GetOwnedBlogAsync(blogId, authorId);
        var addedImages = await SaveImagesAsync(dto.NewImages);

        blog.Edit(dto.Title, dto.Description, dto.KeptImages, addedImages);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveAsync(Guid blogId, Guid authorId)
    {
        var blog = await GetOwnedBlogAsync(blogId, authorId);

        blog.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    // ImageFile and ImageSelection check every image before anything is written to disk.
    // A file saved for a blog that the domain then rejects is removed later by the platform's image cleanup job.
    private async Task<List<string>> SaveImagesAsync(List<ImageUploadDto> uploads)
    {
        var files = uploads
            .Select(upload => new ImageFile(upload.FileName, upload.Content, upload.Width, upload.Height))
            .ToList();

        return await _imageStore.SaveAsync(new ImageSelection(files));
    }

    private async Task<Blog> GetOwnedBlogAsync(Guid blogId, Guid authorId)
    {
        var blog = await _blogRepository.GetByIdAsync(blogId);
        if (blog is null || blog.AuthorId != authorId)
        {
            throw new NotFoundException($"Blog {blogId} does not exist.");
        }
        return blog;
    }
}
