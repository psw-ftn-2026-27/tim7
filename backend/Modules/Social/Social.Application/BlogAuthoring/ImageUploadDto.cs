namespace Social.Application.BlogAuthoring;

public sealed record ImageUploadDto(string FileName, byte[] Content, int Width, int Height);
