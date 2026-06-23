using SACS.Application.Common.Interfaces;

namespace SACS.BackgroundJobs.Services;

public class BackgroundCurrentUserService : ICurrentUserService
{
    public string? UserId => "BackgroundJob";
    public string? Email => "system-job@sacs.com";
}
