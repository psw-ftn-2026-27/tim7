using Games.Application.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizSolving;

public sealed class QuizSolvingQueries
{
    private readonly IQuizReadRepository _quizReadRepository;
    private readonly IQuizAttemptReadRepository _attemptReadRepository;

    public QuizSolvingQueries(IQuizReadRepository quizReadRepository, IQuizAttemptReadRepository attemptReadRepository)
    {
        _quizReadRepository = quizReadRepository;
        _attemptReadRepository = attemptReadRepository;
    }

    public async Task<List<SolvableQuizDto>> GetQuizzesAsync(Guid userId)
    {
        var quizzes = await _quizReadRepository.GetPublishedAsync();
        var attempts = await _attemptReadRepository.GetAllOfUserAsync(userId);

        return quizzes
            .Select(quiz => new SolvableQuizDto(
                quiz.Id,
                quiz.Name,
                StateOf(attempts.SingleOrDefault(attempt => attempt.QuizId == quiz.Id))))
            .ToList();
    }

    // The correct answers are returned only for a solved quiz.
    public async Task<QuizSolvingDto> GetQuizAsync(Guid userId, Guid quizId)
    {
        var quiz = await _quizReadRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }

        var attempt = await _attemptReadRepository.GetAsync(userId, quizId);
        if (attempt is null)
        {
            throw new NotFoundException($"The quiz {quizId} is not started.");
        }

        if (attempt.IsCompleted)
        {
            return new QuizSolvingDto(quiz.Id, quiz.Name, null, ResultOf(quiz, attempt));
        }

        var nextQuestion = quiz.Questions.First(question =>
            attempt.GivenAnswers.All(given => given.QuestionId != question.Id));
        return new QuizSolvingDto(quiz.Id, quiz.Name, WithoutCorrectness(nextQuestion), null);
    }

    private static SolvingQuestionDto WithoutCorrectness(QuestionDto question)
    {
        return new SolvingQuestionDto(
            question.Id,
            question.Text,
            question.AnswerOptions.Select(option => new SolvingAnswerOptionDto(option.Id, option.Text)).ToList());
    }

    private static QuizResultDto ResultOf(QuizDto quiz, QuizAttemptDetailsDto attempt)
    {
        var questions = quiz.Questions
            .Select(question =>
            {
                var given = attempt.GivenAnswers.Single(answer => answer.QuestionId == question.Id);
                var givenOption = question.AnswerOptions.Single(option => option.Id == given.AnswerOptionId);
                var correctOption = question.AnswerOptions.Single(option => option.IsCorrect);
                return new AnsweredQuestionDto(
                    question.Id,
                    question.Text,
                    new SolvingAnswerOptionDto(givenOption.Id, givenOption.Text),
                    new SolvingAnswerOptionDto(correctOption.Id, correctOption.Text));
            })
            .ToList();

        // A solved attempt always has its XP recorded.
        return new QuizResultDto(
            attempt.GivenAnswers.Count(answer => answer.IsCorrect),
            attempt.EarnedXp!.Value,
            questions);
    }

    private static QuizSolvingState StateOf(QuizAttemptDto? attempt)
    {
        if (attempt is null)
        {
            return QuizSolvingState.NotStarted;
        }
        return attempt.IsCompleted ? QuizSolvingState.Solved : QuizSolvingState.Started;
    }
}
