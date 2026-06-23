using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.Events.Queries.GetCalendar;
using SACS.Application.Events.DTOs;

namespace SACS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly ISender _sender;

    public CalendarController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("view")]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetCalendar([FromQuery] string viewType, [FromQuery] DateTime? date)
    {
        var result = await _sender.Send(new GetCalendarQuery(viewType ?? "upcoming", date ?? DateTime.UtcNow));
        return Ok(result);
    }
}
