namespace Shared.Api.Images;

public static class ImageUploadLimits
{
    // 10 images of 5 MB each, plus 1 MB for the other form fields.
    // A module controller puts [RequestSizeLimit(ImageUploadLimits.MaxRequestBytes)] on the action that accepts images.
    public const long MaxRequestBytes = 51L * 1024 * 1024;
}
