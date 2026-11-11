using AutoMapper;
using Exploration.Application.TourPreferences;
using Exploration.Domain.TourPreferences;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourPreferenceSetup;

public sealed class TourPreferenceSetupService
{
    private readonly ITourPreferenceRepository _tourPreferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TourPreferenceSetupService(ITourPreferenceRepository tourPreferenceRepository, IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _tourPreferenceRepository = tourPreferenceRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TourPreferenceDto> SaveMineAsync(Guid userId, SaveTourPreferenceDto dto)
    {
        var tourPreference = await _tourPreferenceRepository.GetByUserAsync(userId);
        if (tourPreference is null)
        {
            tourPreference = new TourPreference(userId, dto.Difficulties, dto.PreferredTransportMode, dto.Tags,
                dto.ApplyPreferencesAutomatically);
            _tourPreferenceRepository.Add(tourPreference);
        }
        else
        {
            tourPreference.Update(dto.Difficulties, dto.PreferredTransportMode, dto.Tags,
                dto.ApplyPreferencesAutomatically);
        }

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TourPreferenceDto>(tourPreference);
    }

    public async Task DeleteMineAsync(Guid userId)
    {
        var tourPreference = await _tourPreferenceRepository.GetByUserAsync(userId);
        if (tourPreference is null)
        {
            throw new NotFoundException($"User {userId} has no tour preferences.");
        }

        _tourPreferenceRepository.Remove(tourPreference);
        await _unitOfWork.SaveChangesAsync();
    }
}
