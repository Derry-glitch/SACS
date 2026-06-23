using System;
using System.Collections.Generic;
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
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.CreateAssignment;

public record CreateAssignmentCommand(
    string Title,
    long CourseOfferingId,
    string Description,
    DateTime DeadlineDate,
    string Priority,
    List<AttachmentRequestDto> Attachments
) : IRequest<AssignmentDto>;

public record AttachmentRequestDto(string FileName, string BlobStorageUrl, long FileSizeInBytes, string ContentType);

public class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.DeadlineDate).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.Priority).Must(p => new[] { "Low", "Medium", "High", "Critical" }.Contains(p));
    }
}

public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, AssignmentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateAssignmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<AssignmentDto> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");
        
        var assignmentMetadata = new EventMetadata { RawDescription = request.Description };

        var newEvent = new AcademicEvent
        {
            Title = request.Title,
            Description = JsonSerializer.Serialize(assignmentMetadata),
            EventType = "Assignment",
            DueDate = request.DeadlineDate,
            Priority = request.Priority,
            CourseOfferingId = request.CourseOfferingId,
            CreatedByUserId = currentUserId,
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await _unitOfWork.Repository<AcademicEvent>().AddAsync(newEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Add attachments
        var responseAttachments = new List<AttachmentDto>();
        foreach (var att in request.Attachments)
        {
            var attachment = new EventAttachment
            {
                AcademicEventId = newEvent.Id,
                FileName = att.FileName,
                BlobStorageUrl = att.BlobStorageUrl,
                FileSizeInBytes = att.FileSizeInBytes,
                ContentType = att.ContentType
            };
            await _unitOfWork.Repository<EventAttachment>().AddAsync(attachment, cancellationToken);
        }

        if (request.Attachments.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Get Course info
        var courseOffering = await _unitOfWork.Repository<CourseSemesterOffering>()
            .Query()
            .Include(co => co.Course)
            .FirstOrDefaultAsync(co => co.Id == request.CourseOfferingId, cancellationToken);

        var courseName = courseOffering?.Course.Code ?? "Unknown Course";

        return new AssignmentDto
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            Description = request.Description,
            CourseOfferingId = newEvent.CourseOfferingId,
            CourseName = courseName,
            DeadlineDate = newEvent.DueDate,
            Priority = newEvent.Priority,
            Attachments = newEvent.Attachments.Select(a => new AttachmentDto
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
