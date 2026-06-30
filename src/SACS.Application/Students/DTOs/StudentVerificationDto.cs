namespace SACS.Application.Students.DTOs;

public class StudentVerificationDto
{
    public bool IsVerified { get; set; }
    public string MatriculationNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Department { get; set; } = null!;
    public int AcademicLevel { get; set; }
    public string InstitutionName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
}
