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
    public async Task<List<string>> SaveAsync(string module, ImageSelection selection)
    {
        var moduleFolder = Path.Combine(Folder, module);
        Directory.CreateDirectory(moduleFolder);

        var addresses = new List<string>();
        foreach (var image in selection.Images)
        {
            var fileName = $"{Guid.NewGuid()}{image.Extension}";
            await File.WriteAllBytesAsync(Path.Combine(moduleFolder, fileName), image.Content);
            addresses.Add(ToAddress(module, fileName));
        }

        return addresses;
    }

    // Lists the files in the module subfolders; files directly in the root folder belong to no module and are not listed.
    public List<StoredImage> ListStored()
    {
        var images = new List<StoredImage>();
        foreach (var moduleFolder in Directory.GetDirectories(Folder))
        {
            var module = Path.GetFileName(moduleFolder);
            foreach (var file in Directory.GetFiles(moduleFolder))
            {
                images.Add(new StoredImage(module, ToAddress(module, Path.GetFileName(file)), File.GetLastWriteTimeUtc(file)));
            }
        }

        return images;
    }

    // Deletes each file on its own, so one failure does not stop the rest. Returns the images that were deleted.
    public List<StoredImage> Delete(IReadOnlyList<StoredImage> images)
    {
        var deleted = new List<StoredImage>();
        foreach (var image in images)
        {
            try
            {
                File.Delete(Path.Combine(Folder, image.Module, Path.GetFileName(image.Address)));
                deleted.Add(image);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        return deleted;
    }

    // The single place the address is built, so the address returned on save and the one listed always match.
    private static string ToAddress(string module, string fileName) => $"/images/{module}/{fileName}";
}
