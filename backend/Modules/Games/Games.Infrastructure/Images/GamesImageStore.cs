using Games.Application;
using Shared.Domain.Images;
using Shared.Infrastructure.Images;

namespace Games.Infrastructure.Images;

internal sealed class GamesImageStore : IImageStore
{
    // The module's subfolder name; GamesImageUsage reports the same name to the cleanup job.
    public const string Module = "games";

    private readonly ImageStore _imageStore;

    public GamesImageStore(ImageStore imageStore)
    {
        _imageStore = imageStore;
    }

    public Task<List<string>> SaveAsync(ImageSelection selection)
    {
        return _imageStore.SaveAsync(Module, selection);
    }
}
