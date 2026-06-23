namespace SACS.Application.Authentication.DTOs;

public class UserDto
{
    public long Id { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public long InstitutionId { get; set; }
    public string? MatriculationNumber { get; set; }
}
