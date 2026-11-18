namespace Social.Application.BlogAuthoring;

// KeptImages are addresses of images the blog already has; new image files travel next to this DTO.
// A form without any kept address binds KeptImages as null, which means the author keeps no image.
public sealed record EditBlogDto(string Title, string Description, List<string>? KeptImages);
