using Exploration.Application.TourExecutions;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Payment.Contracts;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourExecutionTracking;

public sealed class TourExecutionTrackingService
{
    private readonly ITourExecutionRepository _tourExecutionRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentApi _paymentApi;
    private readonly TourExecutionStarter _tourExecutionStarter;

    public TourExecutionTrackingService(
        ITourExecutionRepository tourExecutionRepository,
        ITourRepository tourRepository,
        IUnitOfWork unitOfWork,
        IPaymentApi paymentApi,
        TourExecutionStarter tourExecutionStarter)
    {
        _tourExecutionRepository = tourExecutionRepository;
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
        _paymentApi = paymentApi;
        _tourExecutionStarter = tourExecutionStarter;
    }

    public async Task<Guid> StartTourAsync(Guid touristId, StartTourDto dto)
    {
        var tour = await _tourRepository.GetByIdAsync(dto.TourId)
            ?? throw new NotFoundException($"Tour {dto.TourId} does not exist.");
        var startLocation = new Location(dto.StartLocation.Latitude, dto.StartLocation.Longitude);
        var hasPurchased = await _paymentApi.HasPurchasedAsync(touristId, tour.Id);
        var activeExecution = await _tourExecutionRepository.GetActiveByTouristAsync(touristId);

        var execution = _tourExecutionStarter.Start(touristId, tour, startLocation, hasPurchased, activeExecution);

        // The starter hands back the loaded active session when this tour is already active; only a new session is saved.
        if (execution != activeExecution)
        {
            _tourExecutionRepository.Add(execution);
            await _unitOfWork.SaveChangesAsync();
        }

        return execution.Id;
    }

    public async Task AbandonAsync(Guid touristId)
    {
        var execution = await _tourExecutionRepository.GetActiveByTouristAsync(touristId)
            ?? throw new NotFoundException($"Tourist {touristId} has no active tour.");

        execution.Abandon();
        await _unitOfWork.SaveChangesAsync();
    }
}
