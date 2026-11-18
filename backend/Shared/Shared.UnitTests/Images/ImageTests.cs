using System.Buffers.Binary;
using FluentAssertions;
using Shared.Api.Images;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Xunit;

namespace Shared.UnitTests.Images;

public class ImageTests
{
    private static byte[] CreateImage(ImageFormat format, int width, int height) => format switch
    {
        ImageFormat.Jpeg => CreateJpeg(width, height),
        ImageFormat.Png => CreatePng(width, height),
        _ => CreateWebp(width, height)
    };

    private static byte[] CreateJpeg(int width, int height)
    {
        byte[] content = [0xFF, 0xD8, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0, 0, 0, 0, 0x03];
        BinaryPrimitives.WriteUInt16BigEndian(content.AsSpan(7, 2), (ushort)height);
        BinaryPrimitives.WriteUInt16BigEndian(content.AsSpan(9, 2), (ushort)width);
        return content;
    }

    private static byte[] CreatePng(int width, int height)
    {
        byte[] content = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, .."IHDR"u8, 0, 0, 0, 0, 0, 0, 0, 0];
        BinaryPrimitives.WriteInt32BigEndian(content.AsSpan(16, 4), width);
        BinaryPrimitives.WriteInt32BigEndian(content.AsSpan(20, 4), height);
        return content;
    }

    private static byte[] CreateWebp(int width, int height)
    {
        byte[] content = [.."RIFF"u8, 0x16, 0x00, 0x00, 0x00, .."WEBP"u8, .."VP8X"u8, 0x0A, 0x00, 0x00, 0x00, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
        WriteUInt24LittleEndian(content, 24, width - 1);
        WriteUInt24LittleEndian(content, 27, height - 1);
        return content;
    }

    private static void WriteUInt24LittleEndian(byte[] content, int offset, int value)
    {
        content[offset] = (byte)value;
        content[offset + 1] = (byte)(value >> 8);
        content[offset + 2] = (byte)(value >> 16);
    }

    [Fact]
    public void Content_that_is_not_jpeg_png_or_webp_is_rejected_even_with_an_image_extension()
    {
        var gif = "GIF89a"u8.ToArray();

        var creation = () => new ImageFile("trip.png", gif, 100, 100);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(ImageFormat.Jpeg, ".jpg")]
    [InlineData(ImageFormat.Png, ".png")]
    [InlineData(ImageFormat.Webp, ".webp")]
    public void Format_and_extension_are_derived_from_the_content(ImageFormat format, string extension)
    {
        var content = CreateImage(format, 100, 100);

        var image = new ImageFile("photo.bin", content, 100, 100);

        image.Format.Should().Be(format);
        image.Extension.Should().Be(extension);
    }

    [Fact]
    public void Image_larger_than_5_mb_is_rejected()
    {
        var content = new byte[5 * 1024 * 1024 + 1];
        CreatePng(100, 100).CopyTo(content, 0);

        var creation = () => new ImageFile("trip.png", content, 100, 100);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(6001, 100)]
    [InlineData(100, 6001)]
    public void Image_wider_or_taller_than_6000_px_is_rejected(int width, int height)
    {
        var content = CreatePng(width, height);

        var creation = () => new ImageFile("trip.png", content, width, height);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(100, 0)]
    public void Image_with_unreadable_dimensions_is_rejected(int width, int height)
    {
        var content = CreatePng(100, 100);

        var creation = () => new ImageFile("trip.png", content, width, height);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Selection_of_more_than_10_images_is_rejected()
    {
        var images = Enumerable.Range(0, 11)
            .Select(_ => new ImageFile("trip.png", CreatePng(100, 100), 100, 100))
            .ToList();

        var creation = () => new ImageSelection(images);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(ImageFormat.Jpeg)]
    [InlineData(ImageFormat.Png)]
    [InlineData(ImageFormat.Webp)]
    public void Dimensions_are_read_from_the_file_header(ImageFormat format)
    {
        var content = CreateImage(format, 1920, 1080);

        var read = ImageHeaderReader.TryReadDimensions(content, out var width, out var height);

        read.Should().BeTrue();
        width.Should().Be(1920);
        height.Should().Be(1080);
    }
}
