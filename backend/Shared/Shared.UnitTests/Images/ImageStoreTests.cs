using System.Buffers.Binary;
using FluentAssertions;
using Shared.Domain.Images;
using Shared.Infrastructure.Images;
using Xunit;

namespace Shared.UnitTests.Images;

public class ImageStoreTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"image-store-tests-{Guid.NewGuid()}");

    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
    }

    private static ImageFile CreatePng()
    {
        byte[] content = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, .."IHDR"u8, 0, 0, 0, 0, 0, 0, 0, 0];
        BinaryPrimitives.WriteInt32BigEndian(content.AsSpan(16, 4), 100);
        BinaryPrimitives.WriteInt32BigEndian(content.AsSpan(20, 4), 100);
        return new ImageFile("photo.png", content, 100, 100);
    }

    [Fact]
    public async Task Address_returned_on_save_is_the_address_under_which_the_file_is_listed()
    {
        var store = new ImageStore(_folder);
        var saved = await store.SaveAsync("social", new ImageSelection([CreatePng()]));

        var listed = store.ListStored();

        listed.Select(image => image.Address).Should().Equal(saved);
    }
}
