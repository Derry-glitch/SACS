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

    [HttpGet("day")]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetDailyCalendar([FromQuery] DateTime? date)
    {
        var result = await _sender.Send(new GetDailyCalendarQuery(date));
        return Ok(result);
    }

    [HttpGet("week")]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetWeeklyCalendar([FromQuery] DateTime? date)
    {
        var result = await _sender.Send(new GetWeeklyCalendarQuery(date));
        return Ok(result);
    }

    [HttpGet("month")]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetMonthlyCalendar([FromQuery] DateTime? date)
    {
        var result = await _sender.Send(new GetMonthlyCalendarQuery(date));
        return Ok(result);
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetUpcomingCalendar()
    {
        var result = await _sender.Send(new GetUpcomingCalendarQuery());
        return Ok(result);
    }
}
