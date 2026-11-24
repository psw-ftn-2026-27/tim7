using Games.Application.LocationChallenges;
using Games.Application.QuizManagement;
using Games.Application.Quizzes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "administrator")]
public sealed class QuizManagementController : ControllerBase
{
    private readonly QuizManagementService _managementService;
    private readonly QuizManagementQueries _managementQueries;

    public QuizManagementController(QuizManagementService managementService, QuizManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<ChallengeListItemDto>>> GetChallenges([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetChallengesAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuizDto>> GetQuiz(Guid id)
    {
        return await _managementQueries.GetQuizAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(QuizDetailsDto dto)
    {
        return await _managementService.CreateQuizAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateDetails(Guid id, QuizDetailsDto dto)
    {
        await _managementService.UpdateDetailsAsync(id, dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/location")]
    public async Task<ActionResult> SetLocation(Guid id, LocationDto dto)
    {
        await _managementService.SetLocationAsync(id, dto);
        return NoContent();
    }

    [HttpPost("{id:guid}/questions")]
    public async Task<ActionResult> AddQuestion(Guid id, TextDto dto)
    {
        await _managementService.AddQuestionAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    public async Task<ActionResult> RemoveQuestion(Guid id, Guid questionId)
    {
        await _managementService.RemoveQuestionAsync(id, questionId);
        return NoContent();
    }

    [HttpPost("{id:guid}/questions/{questionId:guid}/answer-options")]
    public async Task<ActionResult> AddAnswerOption(Guid id, Guid questionId, TextDto dto)
    {
        await _managementService.AddAnswerOptionAsync(id, questionId, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/questions/{questionId:guid}/answer-options/{answerOptionId:guid}")]
    public async Task<ActionResult> RemoveAnswerOption(Guid id, Guid questionId, Guid answerOptionId)
    {
        await _managementService.RemoveAnswerOptionAsync(id, questionId, answerOptionId);
        return NoContent();
    }

    [HttpPost("{id:guid}/questions/{questionId:guid}/answer-options/{answerOptionId:guid}/mark-correct")]
    public async Task<ActionResult> MarkAnswerOptionCorrect(Guid id, Guid questionId, Guid answerOptionId)
    {
        await _managementService.MarkAnswerOptionCorrectAsync(id, questionId, answerOptionId);
        return NoContent();
    }
}
