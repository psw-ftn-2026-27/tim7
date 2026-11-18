using System.Buffers.Binary;

namespace Shared.Api.Images;

// Reads the width and height from the first bytes of a JPEG, PNG or WebP file, without decoding the image.
// Returns false when the content is not one of these formats or its header is cut short.
public static class ImageHeaderReader
{
    public static bool TryReadDimensions(byte[] content, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (IsPng(content))
        {
            return TryReadPng(content, out width, out height);
        }

        if (IsJpeg(content))
        {
            return TryReadJpeg(content, out width, out height);
        }

        if (IsWebp(content))
        {
            return TryReadWebp(content, out width, out height);
        }

        return false;
    }

    private static bool IsPng(byte[] content) =>
        StartsWith(content, 0, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);

    private static bool IsJpeg(byte[] content) =>
        StartsWith(content, 0, [0xFF, 0xD8, 0xFF]);

    private static bool IsWebp(byte[] content) =>
        StartsWith(content, 0, "RIFF"u8) && StartsWith(content, 8, "WEBP"u8);

    // PNG: the IHDR chunk follows the 8-byte signature; width and height are big-endian at offsets 16 and 20.
    private static bool TryReadPng(byte[] content, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (content.Length < 24 || !StartsWith(content, 12, "IHDR"u8))
        {
            return false;
        }

        width = BinaryPrimitives.ReadInt32BigEndian(content.AsSpan(16, 4));
        height = BinaryPrimitives.ReadInt32BigEndian(content.AsSpan(20, 4));
        return width > 0 && height > 0;
    }

    // JPEG: walk the marker segments until a start-of-frame (SOF) segment, which holds height then width, big-endian.
    private static bool TryReadJpeg(byte[] content, out int width, out int height)
    {
        width = 0;
        height = 0;
        var position = 2;

        while (position + 1 < content.Length)
        {
            if (content[position] != 0xFF)
            {
                return false;
            }

            var marker = content[position + 1];
            if (marker == 0xFF)
            {
                position++;
                continue;
            }

            if (marker == 0xD9 || marker == 0xDA)
            {
                return false;
            }

            if (marker == 0x01 || (marker >= 0xD0 && marker <= 0xD7))
            {
                position += 2;
                continue;
            }

            if (position + 4 > content.Length)
            {
                return false;
            }

            var segmentLength = BinaryPrimitives.ReadUInt16BigEndian(content.AsSpan(position + 2, 2));
            if (IsStartOfFrame(marker))
            {
                if (position + 9 > content.Length)
                {
                    return false;
                }

                height = BinaryPrimitives.ReadUInt16BigEndian(content.AsSpan(position + 5, 2));
                width = BinaryPrimitives.ReadUInt16BigEndian(content.AsSpan(position + 7, 2));
                return width > 0 && height > 0;
            }

            position += 2 + segmentLength;
        }

        return false;
    }

    // SOF0 to SOF15, except DHT (C4), JPG (C8) and DAC (CC), which share the range but are not frames.
    private static bool IsStartOfFrame(byte marker) =>
        marker >= 0xC0 && marker <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;

    // WebP: the first chunk after "RIFF....WEBP" is VP8X (extended), VP8L (lossless) or "VP8 " (lossy).
    private static bool TryReadWebp(byte[] content, out int width, out int height)
    {
        width = 0;
        height = 0;

        if (StartsWith(content, 12, "VP8X"u8) && content.Length >= 30)
        {
            width = ReadUInt24LittleEndian(content, 24) + 1;
            height = ReadUInt24LittleEndian(content, 27) + 1;
            return true;
        }

        if (StartsWith(content, 12, "VP8L"u8) && content.Length >= 25 && content[20] == 0x2F)
        {
            var bits = BinaryPrimitives.ReadUInt32LittleEndian(content.AsSpan(21, 4));
            width = (int)(bits & 0x3FFF) + 1;
            height = (int)((bits >> 14) & 0x3FFF) + 1;
            return true;
        }

        if (StartsWith(content, 12, "VP8 "u8) && content.Length >= 30 && StartsWith(content, 23, [0x9D, 0x01, 0x2A]))
        {
            width = BinaryPrimitives.ReadUInt16LittleEndian(content.AsSpan(26, 2)) & 0x3FFF;
            height = BinaryPrimitives.ReadUInt16LittleEndian(content.AsSpan(28, 2)) & 0x3FFF;
            return width > 0 && height > 0;
        }

        return false;
    }

    private static int ReadUInt24LittleEndian(byte[] content, int offset) =>
        content[offset] | (content[offset + 1] << 8) | (content[offset + 2] << 16);

    private static bool StartsWith(byte[] content, int offset, ReadOnlySpan<byte> signature) =>
        content.Length >= offset + signature.Length
        && content.AsSpan(offset, signature.Length).SequenceEqual(signature);
}
