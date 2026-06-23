using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SACS.Application.Common.Models;
using SACS.Application.Events.DTOs;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.UpdateAssignment;

public record UpdateAssignmentCommand(
    long Id,
    string Title,
    string Description,
    DateTime DeadlineDate,
    string Priority
) : IRequest<AssignmentDto>;

public class UpdateAssignmentCommandValidator : AbstractValidator<UpdateAssignmentCommand>
{
    public UpdateAssignmentCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.DeadlineDate).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.Priority).Must(p => new[] { "Low", "Medium", "High", "Critical" }.Contains(p));
    }
}

public class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand, AssignmentDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAssignmentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AssignmentDto> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var existingEvent = await _unitOfWork.Repository<AcademicEvent>()
            .Query()
            .Include(a => a.Attachments)
            .Include(a => a.CourseOffering)
                .ThenInclude(co => co.Course)
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.EventType == "Assignment", cancellationToken);

        if (existingEvent == null)
        {
            throw new KeyNotFoundException("Assignment not found.");
        }

        var assignmentMetadata = new EventMetadata { RawDescription = request.Description };

        existingEvent.Title = request.Title;
        existingEvent.Description = JsonSerializer.Serialize(assignmentMetadata);
        existingEvent.DueDate = request.DeadlineDate;
        existingEvent.Priority = request.Priority;

        _unitOfWork.Repository<AcademicEvent>().Update(existingEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AssignmentDto
        {
            Id = existingEvent.Id,
            Title = existingEvent.Title,
            Description = request.Description,
            CourseOfferingId = existingEvent.CourseOfferingId,
            CourseName = existingEvent.CourseOffering.Course.Code,
            DeadlineDate = existingEvent.DueDate,
            Priority = existingEvent.Priority,
            Attachments = existingEvent.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                BlobStorageUrl = a.BlobStorageUrl,
                FileSizeInBytes = a.FileSizeInBytes,
                ContentType = a.ContentType
            }).ToList()
        };
    }
}
