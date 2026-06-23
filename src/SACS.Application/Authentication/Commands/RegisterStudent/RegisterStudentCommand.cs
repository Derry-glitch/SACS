using MediatR;
using SACS.Application.Authentication.DTOs;

namespace SACS.Application.Authentication.Commands.RegisterStudent;

public record RegisterStudentCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string MatriculationNumber,
    int AcademicLevel,
    long InstitutionId,
    string? PhoneNumber = null
) : IRequest<AuthResponseDto>;
