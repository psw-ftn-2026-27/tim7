namespace Exploration.Application.KeyPointAuthoring;

// Bound from the form fields of a multipart request; the image travels beside it as a file.
public sealed record SaveKeyPointDto(string Name, string Description, LocationDto Location, string? Secret, bool IsPublic);
