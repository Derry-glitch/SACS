using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.Events.Commands.ConfigureReminders;
using SACS.Application.Events.Commands.DeleteReminder;
using SACS.Application.Events.Queries.GetMyReminders;
using SACS.Application.Events.DTOs;

namespace SACS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly ISender _sender;

    public RemindersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("configure")]
    public async Task<IActionResult> Configure([FromBody] ConfigureRemindersCommand command)
    {
        await _sender.Send(command);
        return NoContent();
    }

    [HttpGet("my-reminders")]
    public async Task<ActionResult<IEnumerable<ReminderDto>>> GetMyReminders()
    {
        var result = await _sender.Send(new GetMyRemindersQuery());
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _sender.Send(new DeleteReminderCommand(id));
        return NoContent();
    }
}
