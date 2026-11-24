using Games.Application.LocationChallenges;
using Games.Application.Quizzes;
using Games.Domain.LocationChallenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizSolving;

public sealed class QuizSolvingService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizSolvingService(
        IQuizRepository quizRepository,
        IQuizAttemptRepository attemptRepository,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _attemptRepository = attemptRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateQuizAsync(Guid userId, Guid quizId, LocationDto position)
    {
        var quiz = await GetQuizAsync(quizId);
        quiz.EnsureCanBeActivatedFrom(new Location(position.Latitude, position.Longitude));

        var attempt = await _attemptRepository.GetAsync(userId, quizId);
        if (attempt is null)
        {
            _attemptRepository.Add(new QuizAttempt(userId, quizId));
        }
        else
        {
            attempt.EnsureCanActivate();
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AnswerQuestionAsync(Guid userId, Guid quizId, AnswerDto dto)
    {
        var quiz = await GetQuizAsync(quizId);
        var attempt = await _attemptRepository.GetAsync(userId, quizId);
        if (attempt is null)
        {
            throw new NotFoundException($"The quiz {quizId} is not started.");
        }

        var isCorrect = quiz.CheckAnswer(dto.QuestionId, dto.AnswerOptionId);
        // CheckAnswer has just ensured the quiz is published, and a published quiz always has XP.
        attempt.Answer(
            new GivenAnswer(dto.QuestionId, dto.AnswerOptionId, isCorrect),
            quiz.Questions.Select(question => question.Id).ToList(),
            quiz.Xp!.Value);

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Quiz> GetQuizAsync(Guid quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        return quiz;
    }
}
