using Microsoft.AspNetCore.Http;

namespace Shared.Api.Images;

// Called by a module controller while binding the request.
public static class ImageUploadReader
{
    public static async Task<UploadedImage> ReadAsync(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var content = stream.ToArray();

        // An unreadable header hands on 0 x 0, which ImageFile rejects.
        if (!ImageHeaderReader.TryReadDimensions(content, out var width, out var height))
        {
            width = 0;
            height = 0;
        }

        return new UploadedImage(file.FileName, content, width, height);
    }
}
