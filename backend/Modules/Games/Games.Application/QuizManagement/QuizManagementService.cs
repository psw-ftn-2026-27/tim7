using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizManagement;

public sealed class QuizManagementService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizManagementService(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateQuizAsync(QuizDetailsDto dto)
    {
        var quiz = new Quiz(dto.Name, dto.Instructions, dto.Xp);

        _quizRepository.Add(quiz);
        await _unitOfWork.SaveChangesAsync();

        return quiz.Id;
    }

    public async Task UpdateDetailsAsync(Guid quizId, QuizDetailsDto dto)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.UpdateDetails(dto.Name, dto.Instructions, dto.Xp);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddQuestionAsync(Guid quizId, TextDto dto)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.AddQuestion(dto.Text);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveQuestionAsync(Guid quizId, Guid questionId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.RemoveQuestion(questionId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddAnswerOptionAsync(Guid quizId, Guid questionId, TextDto dto)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.AddAnswerOption(questionId, dto.Text);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveAnswerOptionAsync(Guid quizId, Guid questionId, Guid answerOptionId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.RemoveAnswerOption(questionId, answerOptionId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAnswerOptionCorrectAsync(Guid quizId, Guid questionId, Guid answerOptionId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.MarkAnswerOptionCorrect(questionId, answerOptionId);
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
