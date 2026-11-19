using Games.Application.QuizManagement;
using Games.Application.Quizzes;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizReadRepository : IQuizReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<ChallengeListItemDto>> GetChallengesAsync(int page, int pageSize)
    {
        var quizzes = _dbContext.Quizzes.OrderByDescending(quiz => quiz.CreatedAt);

        var totalCount = await quizzes.CountAsync();
        var items = await quizzes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(quiz => new ChallengeListItemDto(
                quiz.Id,
                quiz.Name,
                "quiz",
                quiz.Status,
                quiz.Xp,
                quiz.Questions.Count()))
            .ToListAsync();

        return new PageResult<ChallengeListItemDto>(items, totalCount);
    }

    public Task<QuizDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.Quizzes
            .Where(quiz => quiz.Id == id)
            .Select(quiz => new QuizDto(
                quiz.Id,
                quiz.Name,
                quiz.Instructions,
                quiz.Xp,
                quiz.Status,
                quiz.Questions
                    .Select(question => new QuestionDto(
                        question.Id,
                        question.Text,
                        question.AnswerOptions
                            .Select(option => new AnswerOptionDto(option.Id, option.Text, option.IsCorrect))
                            .ToList()))
                    .ToList()))
            .FirstOrDefaultAsync();
    }
}
