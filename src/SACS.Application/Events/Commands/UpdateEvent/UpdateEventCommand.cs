using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Models;
using SACS.Application.Events.DTOs;
using SACS.Domain.Common;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.UpdateEvent;

public record UpdateEventCommand(
    long Id,
    string Title,
    string? Description,
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

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.DueDateTime).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.PriorityLevel).Must(p => new[] { "Low", "Medium", "High", "Critical" }.Contains(p));
    }
}

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, EventDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEventCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EventDto> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var existingEvent = await _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(ae => ae.CourseOffering)
                .ThenInclude(co => co.Course)
            .FirstOrDefaultAsync(ae => ae.Id == request.Id, cancellationToken);

        if (existingEvent == null)
        {
            throw new KeyNotFoundException("Academic event not found.");
        }

        Enum.TryParse<AcademicEventType>(existingEvent.EventType, out var eventType);

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

        existingEvent.Title = request.Title;
        existingEvent.Description = JsonSerializer.Serialize(metadata);
        existingEvent.DueDate = request.DueDateTime;
        existingEvent.Venue = request.Venue;
        existingEvent.Priority = request.PriorityLevel;

        _unitOfWork.Repository<AcademicEvent>().Update(existingEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EventDto
        {
            Id = existingEvent.Id,
            Title = existingEvent.Title,
            Description = request.Description,
            CourseId = existingEvent.CourseOfferingId,
            CourseCode = existingEvent.CourseOffering.Course.Code,
            EventType = eventType,
            DueDateTime = existingEvent.DueDate,
            PriorityLevel = existingEvent.Priority,
            Notes = request.Notes,
            AttachmentUrl = request.AttachmentUrl,
            CreatedByUserId = existingEvent.CreatedByUserId,
            DurationMinutes = request.DurationMinutes,
            Venue = existingEvent.Venue,
            SeatNumber = request.SeatNumber,
            SupervisorName = request.SupervisorName,
            ProgressPercentage = request.ProgressPercentage,
            StudyTopic = request.StudyTopic,
            StudyDuration = request.StudyDuration
        };
    }
}
