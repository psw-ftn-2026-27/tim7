using Shared.Domain.Images;
using Shared.Infrastructure.Images;
using Social.Application;

namespace Social.Infrastructure.Images;

internal sealed class SocialImageStore : IImageStore
{
    // The module's subfolder name; SocialImageUsage reports the same name to the cleanup job.
    public const string Module = "social";

    private readonly ImageStore _imageStore;

    public SocialImageStore(ImageStore imageStore)
    {
        _imageStore = imageStore;
    }

    public Task<List<string>> SaveAsync(ImageSelection selection)
    {
        return _imageStore.SaveAsync(Module, selection);
    }
}
