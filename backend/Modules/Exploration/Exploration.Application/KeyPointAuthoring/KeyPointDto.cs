namespace Exploration.Application.KeyPointAuthoring;

public sealed record KeyPointDto(
    Guid Id,
    int Order,
    string Name,
    string Description,
    string Image,
    LocationDto Location,
    string? Secret,
    bool IsPublic);
