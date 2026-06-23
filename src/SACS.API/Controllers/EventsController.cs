using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.Common.Interfaces;
using SACS.Application.Events.Commands.CreateEvent;
using SACS.Application.Events.Commands.UpdateEvent;
using SACS.Application.Events.Commands.DeleteEvent;
using SACS.Application.Events.Queries.GetEventById;
using SACS.Application.Events.Queries.GetEvents;
using SACS.Application.Events.DTOs;

namespace SACS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IBlobStorageService _blobStorageService;

    public EventsController(ISender sender, IBlobStorageService blobStorageService)
    {
        _sender = sender;
        _blobStorageService = blobStorageService;
    }

    [HttpPost("create")]
    public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll()
    {
        var result = await _sender.Send(new GetEventsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetById(long id)
    {
        var result = await _sender.Send(new GetEventByIdQuery(id));
        return Ok(result);
    }

    [HttpPut("update/{id}")]
    public async Task<ActionResult<EventDto>> Update(long id, [FromBody] UpdateEventCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _sender.Send(new DeleteEventCommand(id));
        return NoContent();
    }

    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty");
        }

        using var stream = file.OpenReadStream();
        var fileName = $"{System.Guid.NewGuid()}_{file.FileName}";
        var fileUrl = await _blobStorageService.UploadAsync(stream, fileName, "event-attachments", file.ContentType);
        return Ok(new { attachmentUrl = fileUrl });
    }
}
