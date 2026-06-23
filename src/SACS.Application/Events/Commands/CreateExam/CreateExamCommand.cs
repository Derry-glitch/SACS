using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Interfaces;
using SACS.Application.Common.Models;
using SACS.Application.Events.DTOs;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.CreateExam;

public record CreateExamCommand(
    string Title,
    long CourseOfferingId,
    DateTime ExamDate,
    string Venue,
    int DurationMinutes,
    string? SeatNumber = null,
    string? Notes = null
) : IRequest<ExamDto>;

public class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.ExamDate).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.Venue).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
    }
}

public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, ExamDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateExamCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ExamDto> Handle(CreateExamCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        var examMetadata = new EventMetadata
        {
            DurationMinutes = request.DurationMinutes,
            SeatNumber = request.SeatNumber,
            Notes = request.Notes
        };

        var newEvent = new AcademicEvent
        {
            Title = request.Title,
            Description = JsonSerializer.Serialize(examMetadata),
            EventType = "Exam",
            DueDate = request.ExamDate,
            Venue = request.Venue,
            CourseOfferingId = request.CourseOfferingId,
            CreatedByUserId = currentUserId,
            Priority = "High",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await _unitOfWork.Repository<AcademicEvent>().AddAsync(newEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var courseOffering = await _unitOfWork.Repository<CourseSemesterOffering>()
            .Query()
            .Include(co => co.Course)
            .FirstOrDefaultAsync(co => co.Id == request.CourseOfferingId, cancellationToken);

        return new ExamDto
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            CourseOfferingId = newEvent.CourseOfferingId,
            CourseName = courseOffering?.Course.Code ?? "Unknown Course",
            ExamDate = newEvent.DueDate,
            Venue = newEvent.Venue,
            DurationMinutes = request.DurationMinutes,
            SeatNumber = request.SeatNumber,
            Notes = request.Notes
        };
    }
}
