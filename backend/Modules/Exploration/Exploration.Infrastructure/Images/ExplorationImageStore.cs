using Exploration.Application;
using Shared.Domain.Images;
using Shared.Infrastructure.Images;

namespace Exploration.Infrastructure.Images;

internal sealed class ExplorationImageStore : IImageStore
{
    // The module's subfolder in the shared image store, also reported by ExplorationImageUsage.
    public const string Module = "exploration";

    private readonly ImageStore _imageStore;

    public ExplorationImageStore(ImageStore imageStore)
    {
        _imageStore = imageStore;
    }

    public Task<List<string>> SaveAsync(ImageSelection selection)
    {
        return _imageStore.SaveAsync(Module, selection);
    }
}
