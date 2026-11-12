using Shared.Domain.Images;

namespace Shared.Infrastructure.Images;

// Public because a module's Infrastructure implements its own IImageStore port by calling this class.
public sealed class ImageStore
{
    public string Folder { get; }

    public ImageStore(string folder)
    {
        Folder = Path.GetFullPath(folder);
        Directory.CreateDirectory(Folder);
    }

    // The file name is generated here and never taken from the user.
    public async Task<List<string>> SaveAsync(ImageSelection selection)
    {
        var addresses = new List<string>();
        foreach (var image in selection.Images)
        {
            var fileName = $"{Guid.NewGuid()}{image.Extension}";
            await File.WriteAllBytesAsync(Path.Combine(Folder, fileName), image.Content);
            addresses.Add($"/images/{fileName}");
        }

        return addresses;
    }
}
