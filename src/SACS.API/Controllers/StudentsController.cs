using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SACS.Application.Students.Queries.VerifyStudent;
using SACS.Application.Students.DTOs;

namespace SACS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ISender _sender;

    public StudentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("verify/{matriculationNumber}")]
    public async Task<ActionResult<StudentVerificationDto>> Verify(string matriculationNumber)
    {
        var query = new VerifyStudentQuery(matriculationNumber);
        var result = await _sender.Send(query);
        return Ok(result);
    }
}
