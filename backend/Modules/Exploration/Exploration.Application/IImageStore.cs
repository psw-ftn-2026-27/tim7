using Shared.Domain.Images;

namespace Exploration.Application;

// Implemented in Infrastructure, which saves the images in the module's own subfolder and returns their addresses.
public interface IImageStore
{
    Task<List<string>> SaveAsync(ImageSelection selection);
}
