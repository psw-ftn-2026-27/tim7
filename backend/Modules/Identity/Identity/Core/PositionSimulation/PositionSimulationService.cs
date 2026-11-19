using Identity.Core.UserPositions;

namespace Identity.Core.PositionSimulation;

public sealed class PositionSimulationService
{
    private readonly IUserPositionRepository _userPositionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PositionSimulationService(IUserPositionRepository userPositionRepository, IUnitOfWork unitOfWork)
    {
        _userPositionRepository = userPositionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SetPositionAsync(Guid userId, SetPositionDto dto)
    {
        var coordinates = new Coordinates(dto.Latitude, dto.Longitude);

        var userPosition = await _userPositionRepository.GetByUserAsync(userId);
        if (userPosition is null)
        {
            userPosition = new UserPosition(userId, coordinates);
            _userPositionRepository.Add(userPosition);
        }
        else
        {
            userPosition.MoveTo(coordinates);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
