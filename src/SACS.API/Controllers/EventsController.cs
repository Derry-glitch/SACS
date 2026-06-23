using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.Events.Commands.CreateAssignment;
using SACS.Application.Events.Commands.CreateExam;
using SACS.Application.Events.Commands.CreateQuiz;
using SACS.Application.Events.Commands.CreateProject;
using SACS.Application.Events.Commands.UpdateAssignment;
using SACS.Application.Events.Commands.DeleteAssignment;
using SACS.Application.Events.Commands.SetReminders;
using SACS.Application.Events.Queries.GetAssignments;
using SACS.Application.Events.Queries.GetExams;
using SACS.Application.Events.Queries.GetQuizzes;
using SACS.Application.Events.Queries.GetProjects;
using SACS.Application.Events.DTOs;

namespace SACS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ISender _sender;

    public EventsController(ISender sender)
    {
        _sender = sender;
    }

    // Assignment Management
    [HttpPost("assignment/create")]
    public async Task<ActionResult<AssignmentDto>> CreateAssignment([FromBody] CreateAssignmentCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpGet("assignment/all")]
    public async Task<ActionResult<IEnumerable<AssignmentDto>>> GetAllAssignments()
    {
        var result = await _sender.Send(new GetAssignmentsQuery());
        return Ok(result);
    }

    [HttpPut("assignment/update")]
    public async Task<ActionResult<AssignmentDto>> UpdateAssignment([FromBody] UpdateAssignmentCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpDelete("assignment/delete/{id}")]
    public async Task<IActionResult> DeleteAssignment(long id)
    {
        await _sender.Send(new DeleteAssignmentCommand(id));
        return NoContent();
    }

    // Quiz Management
    [HttpPost("quiz/create")]
    public async Task<ActionResult<QuizDto>> CreateQuiz([FromBody] CreateQuizCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpGet("quiz/all")]
    public async Task<ActionResult<IEnumerable<QuizDto>>> GetAllQuizzes()
    {
        var result = await _sender.Send(new GetQuizzesQuery());
        return Ok(result);
    }

    // Exam Management
    [HttpPost("exam/create")]
    public async Task<ActionResult<ExamDto>> CreateExam([FromBody] CreateExamCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpGet("exam/all")]
    public async Task<ActionResult<IEnumerable<ExamDto>>> GetAllExams()
    {
        var result = await _sender.Send(new GetExamsQuery());
        return Ok(result);
    }

    // Project Submission Management
    [HttpPost("project/create")]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpGet("project/all")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAllProjects()
    {
        var result = await _sender.Send(new GetProjectsQuery());
        return Ok(result);
    }

    // Reminders
    [HttpPost("reminders/set")]
    public async Task<IActionResult> SetReminders([FromBody] SetRemindersCommand command)
    {
        await _sender.Send(command);
        return NoContent();
    }
}
