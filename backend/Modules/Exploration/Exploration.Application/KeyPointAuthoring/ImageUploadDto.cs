namespace Exploration.Application.KeyPointAuthoring;

public sealed record ImageUploadDto(string FileName, byte[] Content, int Width, int Height);
