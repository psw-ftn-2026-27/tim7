namespace Social.Application.BlogAuthoring;

// KeptImages are addresses of images the blog already has; NewImages are files uploaded with this edit.
public sealed record EditBlogDto(string Title, string Description, List<string> KeptImages, List<ImageUploadDto> NewImages);
