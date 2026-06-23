using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Interfaces;
using SACS.Application.Common.Models;
using SACS.Application.Events.DTOs;
using SACS.Domain.Common;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.CreateEvent;

public record CreateEventCommand(
    string Title,
    string? Description,
    long CourseId,
    AcademicEventType EventType,
    DateTime DueDateTime,
    string PriorityLevel,
    string? Notes,
    string? AttachmentUrl,
    
    // Type-specific
    int? DurationMinutes = null,
    string? Venue = null,
    string? SeatNumber = null,
    string? SupervisorName = null,
    int? ProgressPercentage = null,
    string? StudyTopic = null,
    int? StudyDuration = null
) : IRequest<EventDto>;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CourseId).GreaterThan(0);
        RuleFor(x => x.DueDateTime).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.PriorityLevel).Must(p => new[] { "Low", "Medium", "High", "Critical" }.Contains(p));
        
        RuleFor(x => x.DurationMinutes).GreaterThan(0).When(x => x.EventType == AcademicEventType.Quiz || x.EventType == AcademicEventType.Exam);
        RuleFor(x => x.Venue).NotEmpty().When(x => x.EventType == AcademicEventType.Exam);
        RuleFor(x => x.SupervisorName).NotEmpty().When(x => x.EventType == AcademicEventType.Project);
        RuleFor(x => x.ProgressPercentage).InclusiveBetween(0, 100).When(x => x.EventType == AcademicEventType.Project);
        RuleFor(x => x.StudyTopic).NotEmpty().When(x => x.EventType == AcademicEventType.StudySession);
        RuleFor(x => x.StudyDuration).GreaterThan(0).When(x => x.EventType == AcademicEventType.StudySession);
    }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateEventCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<EventDto> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        var metadata = new EventMetadata
        {
            Notes = request.Notes,
            AttachmentUrl = request.AttachmentUrl,
            DurationMinutes = request.DurationMinutes,
            SeatNumber = request.SeatNumber,
            SupervisorName = request.SupervisorName,
            ProgressPercentage = request.ProgressPercentage,
            StudyTopic = request.StudyTopic,
            StudyDuration = request.StudyDuration
        };

        var academicEvent = new AcademicEvent
        {
            Title = request.Title,
            Description = JsonSerializer.Serialize(metadata),
            EventType = request.EventType.ToString(),
            DueDate = request.DueDateTime,
            Venue = request.Venue,
            CourseOfferingId = request.CourseId,
            CreatedByUserId = currentUserId,
            Priority = request.PriorityLevel,
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await _unitOfWork.Repository<AcademicEvent>().AddAsync(academicEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Handle attachment mapping in EventAttachment table if attachment URL is provided
        if (!string.IsNullOrEmpty(request.AttachmentUrl))
        {
            var attachment = new EventAttachment
            {
                AcademicEventId = academicEvent.Id,
                FileName = Path.GetFileName(request.AttachmentUrl),
                BlobStorageUrl = request.AttachmentUrl,
                FileSizeInBytes = 0,
                ContentType = "application/octet-stream"
            };
            await _unitOfWork.Repository<EventAttachment>().AddAsync(attachment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Automatically schedule reminders based on user's default notification preferences
        var preferences = await _unitOfWork.Repository<NotificationPreference>()
            .Query()
            .Where(np => np.UserId == currentUserId && np.IsEnabled)
            .ToListAsync(cancellationToken);

        // If no custom preferences are configured, default to TwentyFourHour (24 hours before)
        if (!preferences.Any())
        {
            preferences = new List<NotificationPreference>
            {
                new() { UserId = currentUserId, ReminderType = "TwentyFourHour", IsEnabled = true }
            };
        }

        foreach (var pref in preferences)
        {
            DateTime? scheduledTime = pref.ReminderType.ToLowerInvariant() switch
            {
                "twentyfourhour" or "24hours" => academicEvent.DueDate.AddDays(-1),
                "threeday" or "72hours" => academicEvent.DueDate.AddDays(-3),
                "sevenday" or "7days" => academicEvent.DueDate.AddDays(-7),
                _ => null
            };

            if (scheduledTime.HasValue && scheduledTime.Value > DateTime.UtcNow)
            {
                var notification = new ScheduledNotification
                {
                    UserId = currentUserId,
                    AcademicEventId = academicEvent.Id,
                    ReminderType = pref.ReminderType,
                    ScheduledTime = scheduledTime.Value,
                    Status = "Pending"
                };
                await _unitOfWork.Repository<ScheduledNotification>().AddAsync(notification, cancellationToken);
            }
        }

        if (preferences.Any())
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var courseOffering = await _unitOfWork.Repository<CourseSemesterOffering>()
            .Query()
            .Include(co => co.Course)
            .FirstOrDefaultAsync(co => co.Id == request.CourseId, cancellationToken);

        return new EventDto
        {
            Id = academicEvent.Id,
            Title = academicEvent.Title,
            Description = request.Description,
            CourseId = academicEvent.CourseOfferingId,
            CourseCode = courseOffering?.Course.Code ?? "Unknown",
            EventType = request.EventType,
            DueDateTime = academicEvent.DueDate,
            PriorityLevel = academicEvent.Priority,
            Notes = request.Notes,
            AttachmentUrl = request.AttachmentUrl,
            CreatedByUserId = academicEvent.CreatedByUserId,
            DurationMinutes = request.DurationMinutes,
            Venue = academicEvent.Venue,
            SeatNumber = request.SeatNumber,
            SupervisorName = request.SupervisorName,
            ProgressPercentage = request.ProgressPercentage,
            StudyTopic = request.StudyTopic,
            StudyDuration = request.StudyDuration
        };
    }
}
