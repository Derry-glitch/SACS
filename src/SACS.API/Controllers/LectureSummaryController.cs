using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.AI.Commands.SummarizeLectureNotes;

namespace SACS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LectureSummaryController : ControllerBase
{
    private readonly ISender _sender;

    public LectureSummaryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("summarize")]
    public async Task<IActionResult> Summarize(IFormFile file, [FromForm] long courseOfferingId)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty or not provided.");
        }

        using var stream = file.OpenReadStream();
        var command = new SummarizeLectureNotesCommand(
            FileStream: stream,
            FileName: file.FileName,
            ContentType: file.ContentType,
            CourseOfferingId: courseOfferingId,
            FileSizeInBytes: file.Length
        );

        var fileRecordId = await _sender.Send(command);
        return Accepted(new { FileRecordId = fileRecordId, Status = "Pending", Message = "Lecture note summarization job queued successfully." });
    }
}
