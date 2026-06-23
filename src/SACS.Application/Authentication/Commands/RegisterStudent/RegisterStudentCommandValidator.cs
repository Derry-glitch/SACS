using FluentValidation;

namespace SACS.Application.Authentication.Commands.RegisterStudent;

public class RegisterStudentCommandValidator : AbstractValidator<RegisterStudentCommand>
{
    public RegisterStudentCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MatriculationNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.AcademicLevel).GreaterThan(0);
        RuleFor(x => x.InstitutionId).GreaterThan(0);
        RuleFor(x => x.PhoneNumber).MaximumLength(20);
    }
}
