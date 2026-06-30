using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.Common.Interfaces;

namespace SACS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAiServiceClient _aiServiceClient;

    public AIController(IAiServiceClient aiServiceClient)
    {
        _aiServiceClient = aiServiceClient;
    }

    public record SummarizeTextRequest(string Text);

    [HttpPost("summarize-text")]
    public async Task<IActionResult> SummarizeText([FromBody] SummarizeTextRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text content is required.");
        }

        var result = await _aiServiceClient.SummarizeTextAsync(request.Text);
        return Ok(result);
    }

    public record GenerateQuizRequest(string Content, string DifficultyLevel);

    [HttpPost("generate-quiz")]
    public async Task<IActionResult> GenerateQuiz([FromBody] GenerateQuizRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest("Content is required for quiz generation.");
        }

        var difficulty = string.IsNullOrWhiteSpace(request.DifficultyLevel) ? "Medium" : request.DifficultyLevel;
        var result = await _aiServiceClient.GenerateQuizAsync(request.Content, difficulty);
        return Ok(result);
    }

    [HttpPost("generate-study-plan")]
    public async Task<IActionResult> GenerateStudyPlan([FromBody] StudyPlanRequestDto request)
    {
        if (request == null)
        {
            return BadRequest("Study plan request payload is required.");
        }

        var result = await _aiServiceClient.GenerateStudyPlanAsync(request);
        return Ok(result);
    }
}
