using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.AI.Commands.GenerateQuiz;

namespace SACS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class QuizGenerationController : ControllerBase
{
    private readonly ISender _sender;

    public QuizGenerationController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateQuizCommand command)
    {
        var jobId = await _sender.Send(command);
        return Accepted(new { JobId = jobId, Status = "Pending", Message = "AI quiz generation has been enqueued." });
    }
}
