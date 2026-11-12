using Shared.Domain.Exceptions;

namespace Shared.Domain.Images;

public sealed record ImageFile
{
    private const int MaxSizeInBytes = 5 * 1024 * 1024;
    private const int MaxSideInPixels = 6000;

    public byte[] Content { get; }
    public ImageFormat Format { get; }
    public int Width { get; }
    public int Height { get; }
    public string Extension => Format switch
    {
        ImageFormat.Jpeg => ".jpg",
        ImageFormat.Png => ".png",
        _ => ".webp"
    };

    // originalName is used only in error messages and is not kept, so the user's file name never reaches storage.
    public ImageFile(string originalName, byte[] content, int width, int height)
    {
        if (content.Length > MaxSizeInBytes)
        {
            throw new DomainException($"Image '{originalName}' is larger than 5 MB.");
        }

        Format = DetectFormat(content)
            ?? throw new DomainException($"Image '{originalName}' is not a JPEG, PNG or WebP image.");

        if (width < 1 || height < 1)
        {
            throw new DomainException($"Image '{originalName}' has unreadable dimensions.");
        }

        if (width > MaxSideInPixels || height > MaxSideInPixels)
        {
            throw new DomainException($"Image '{originalName}' is wider or taller than 6000 px.");
        }

        Content = content;
        Width = width;
        Height = height;
    }

    private static ImageFormat? DetectFormat(byte[] content)
    {
        if (StartsWith(content, 0, [0xFF, 0xD8, 0xFF]))
        {
            return ImageFormat.Jpeg;
        }

        if (StartsWith(content, 0, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return ImageFormat.Png;
        }

        // "RIFF", four bytes of file size, then "WEBP".
        if (StartsWith(content, 0, [0x52, 0x49, 0x46, 0x46]) && StartsWith(content, 8, [0x57, 0x45, 0x42, 0x50]))
        {
            return ImageFormat.Webp;
        }

        return null;
    }

    private static bool StartsWith(byte[] content, int offset, byte[] signature)
    {
        return content.Length >= offset + signature.Length
            && content.AsSpan(offset, signature.Length).SequenceEqual(signature);
    }
}
