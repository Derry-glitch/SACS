using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.AI.Commands.ExtractDeadline;

namespace SACS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DeadlineExtractionController : ControllerBase
{
    private readonly ISender _sender;

    public DeadlineExtractionController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("extract")]
    public async Task<IActionResult> Extract([FromBody] ExtractDeadlineCommand command)
    {
        var ingestedMessageId = await _sender.Send(command);
        return Accepted(new { IngestedMessageId = ingestedMessageId, Status = "Pending", Message = "Deadline extraction job queued successfully." });
    }
}
