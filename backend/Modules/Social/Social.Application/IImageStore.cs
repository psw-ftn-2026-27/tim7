using Shared.Domain.Images;

namespace Social.Application;

// Declared here and implemented in Infrastructure, so the application layer saves images without depending on the disk.
public interface IImageStore
{
    Task<List<string>> SaveAsync(ImageSelection selection);
}
