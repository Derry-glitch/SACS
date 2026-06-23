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

namespace SACS.Application.Events.Commands.CreateQuiz;

public record CreateQuizCommand(
    string Title,
    long CourseOfferingId,
    DateTime Date,
    int DurationMinutes,
    string ReminderWindow,
    string? Notes = null
) : IRequest<QuizDto>;

public class CreateQuizCommandValidator : AbstractValidator<CreateQuizCommand>
{
    public CreateQuizCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.Date).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.ReminderWindow).NotEmpty();
    }
}

public class CreateQuizCommandHandler : IRequestHandler<CreateQuizCommand, QuizDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateQuizCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<QuizDto> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");
        
        var quizMetadata = new EventMetadata 
        { 
            DurationMinutes = request.DurationMinutes,
            ReminderWindow = request.ReminderWindow,
            Notes = request.Notes
        };

        var newEvent = new AcademicEvent
        {
            Title = request.Title,
            Description = JsonSerializer.Serialize(quizMetadata),
            EventType = "Quiz",
            DueDate = request.Date,
            CourseOfferingId = request.CourseOfferingId,
            CreatedByUserId = currentUserId,
            Priority = "Medium",
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

        return new QuizDto
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            CourseOfferingId = newEvent.CourseOfferingId,
            CourseName = courseOffering?.Course.Code ?? "Unknown Course",
            Date = newEvent.DueDate,
            DurationMinutes = request.DurationMinutes,
            ReminderWindow = request.ReminderWindow,
            Notes = request.Notes
        };
    }
}
