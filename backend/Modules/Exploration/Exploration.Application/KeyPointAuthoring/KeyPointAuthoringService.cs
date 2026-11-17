using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;

namespace Exploration.Application.KeyPointAuthoring;

public sealed class KeyPointAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStore _imageStore;

    public KeyPointAuthoringService(ITourRepository tourRepository, IUnitOfWork unitOfWork, IImageStore imageStore)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
        _imageStore = imageStore;
    }

    public async Task AddKeyPointAsync(Guid tourId, Guid authorId, SaveKeyPointDto dto, ImageUploadDto image)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var location = new Location(dto.Location.Latitude, dto.Location.Longitude);
        var imageAddress = await SaveImageAsync(image);

        tour.AddKeyPoint(dto.Name, dto.Description, imageAddress, location, dto.Secret, dto.IsPublic);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeKeyPointAsync(Guid tourId, Guid authorId, Guid keyPointId, SaveKeyPointDto dto, ImageUploadDto? image)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var location = new Location(dto.Location.Latitude, dto.Location.Longitude);
        var imageAddress = image is null ? null : await SaveImageAsync(image);

        tour.ChangeKeyPoint(keyPointId, dto.Name, dto.Description, imageAddress, location, dto.Secret, dto.IsPublic);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveKeyPointAsync(Guid tourId, Guid authorId, Guid keyPointId)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);

        tour.RemoveKeyPoint(keyPointId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<string> SaveImageAsync(ImageUploadDto image)
    {
        var file = new ImageFile(image.FileName, image.Content, image.Width, image.Height);
        var addresses = await _imageStore.SaveAsync(new ImageSelection([file]));
        return addresses[0];
    }

    private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.AuthorId != authorId)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return tour;
    }
}
