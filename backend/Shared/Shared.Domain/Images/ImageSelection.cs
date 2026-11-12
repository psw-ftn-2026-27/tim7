using Shared.Domain.Exceptions;

namespace Shared.Domain.Images;

public sealed record ImageSelection
{
    private const int MaxImages = 10;

    public IReadOnlyList<ImageFile> Images { get; }

    public ImageSelection(List<ImageFile> images)
    {
        if (images.Count > MaxImages)
        {
            throw new DomainException("A selection cannot have more than 10 images.");
        }

        Images = images.ToList();
    }
}
