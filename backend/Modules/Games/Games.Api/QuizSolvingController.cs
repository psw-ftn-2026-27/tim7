using Games.Application.LocationChallenges;
using Games.Application.QuizSolving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes/solving")]
[Authorize(Roles = "explorer")]
public sealed class QuizSolvingController : ControllerBase
{
    private readonly QuizSolvingService _solvingService;
    private readonly QuizSolvingQueries _solvingQueries;

    public QuizSolvingController(QuizSolvingService solvingService, QuizSolvingQueries solvingQueries)
    {
        _solvingService = solvingService;
        _solvingQueries = solvingQueries;
    }

    [HttpGet]
    public async Task<ActionResult<List<SolvableQuizDto>>> GetAll()
    {
        return await _solvingQueries.GetQuizzesAsync(User.GetUserId());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuizSolvingDto>> Get(Guid id)
    {
        return await _solvingQueries.GetQuizAsync(User.GetUserId(), id);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id, [FromBody] LocationDto position)
    {
        await _solvingService.ActivateQuizAsync(User.GetUserId(), id, position);
        return NoContent();
    }

    [HttpPost("{id:guid}/answers")]
    public async Task<ActionResult> Answer(Guid id, [FromBody] AnswerDto answer)
    {
        await _solvingService.AnswerQuestionAsync(User.GetUserId(), id, answer);
        return NoContent();
    }
}
