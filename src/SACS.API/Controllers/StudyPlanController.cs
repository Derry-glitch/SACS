using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.AI.Commands.GenerateStudyPlan;

namespace SACS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StudyPlanController : ControllerBase
{
    private readonly ISender _sender;

    public StudyPlanController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateStudyPlanCommand command)
    {
        var jobId = await _sender.Send(command);
        return Accepted(new { JobId = jobId, Status = "Pending", Message = "AI study plan generation has been enqueued." });
    }
}
