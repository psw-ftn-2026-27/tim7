namespace Shared.Api.Images;

// The neutral data of one uploaded file. A module controller copies it into its own Application DTO.
public sealed record UploadedImage(string FileName, byte[] Content, int Width, int Height);
