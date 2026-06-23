using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace SACS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected long CurrentUserId => long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    protected long CurrentInstitutionId => long.TryParse(User.FindFirst("InstitutionId")?.Value, out var id) ? id : 0;
}
