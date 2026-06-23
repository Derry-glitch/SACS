using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Models;
using SACS.Application.Events.DTOs;
using SACS.Domain.Common;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Queries.GetEventById;

public record GetEventByIdQuery(long Id) : IRequest<EventDto>;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEventByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EventDto> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
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

        var meta = new EventMetadata();
        try
        {
            meta = JsonSerializer.Deserialize<EventMetadata>(existingEvent.Description ?? "") ?? new EventMetadata();
        }
        catch { }

        return new EventDto
        {
            Id = existingEvent.Id,
            Title = existingEvent.Title,
            Description = existingEvent.Description,
            CourseId = existingEvent.CourseOfferingId,
            CourseCode = existingEvent.CourseOffering.Course.Code,
            EventType = eventType,
            DueDateTime = existingEvent.DueDate,
            PriorityLevel = existingEvent.Priority,
            Notes = meta.Notes,
            AttachmentUrl = meta.AttachmentUrl,
            CreatedByUserId = existingEvent.CreatedByUserId,
            DurationMinutes = meta.DurationMinutes,
            Venue = existingEvent.Venue,
            SeatNumber = meta.SeatNumber,
            SupervisorName = meta.SupervisorName,
            ProgressPercentage = meta.ProgressPercentage,
            StudyTopic = meta.StudyTopic,
            StudyDuration = meta.StudyDuration
        };
    }
}
