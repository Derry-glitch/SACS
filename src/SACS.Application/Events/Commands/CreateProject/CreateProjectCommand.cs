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
using SACS.Application.Events.Commands.CreateAssignment;
using SACS.Application.Events.DTOs;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.Events.Commands.CreateProject;

public record CreateProjectCommand(
    string Title,
    long CourseOfferingId,
    string SupervisorName,
    DateTime SubmissionDate,
    int ProgressPercentage,
    string? Notes = null,
    List<AttachmentRequestDto>? Attachments = null
) : IRequest<ProjectDto>;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.CourseOfferingId).GreaterThan(0);
        RuleFor(x => x.SupervisorName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SubmissionDate).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.ProgressPercentage).InclusiveBetween(0, 100);
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = long.Parse(_currentUserService.UserId ?? "0");

        var projectMetadata = new EventMetadata
        {
            SupervisorName = request.SupervisorName,
            ProgressPercentage = request.ProgressPercentage,
            Notes = request.Notes
        };

        var newEvent = new AcademicEvent
        {
            Title = request.Title,
            Description = JsonSerializer.Serialize(projectMetadata),
            EventType = "Project",
            DueDate = request.SubmissionDate,
            CourseOfferingId = request.CourseOfferingId,
            CreatedByUserId = currentUserId,
            Priority = "Medium",
            Status = "Active",
            SourceType = "Manual",
            IsVisibleToStudents = true
        };

        await _unitOfWork.Repository<AcademicEvent>().AddAsync(newEvent, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.Attachments != null)
        {
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
        }

        var courseOffering = await _unitOfWork.Repository<CourseSemesterOffering>()
            .Query()
            .Include(co => co.Course)
            .FirstOrDefaultAsync(co => co.Id == request.CourseOfferingId, cancellationToken);

        return new ProjectDto
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            CourseOfferingId = newEvent.CourseOfferingId,
            CourseName = courseOffering?.Course.Code ?? "Unknown Course",
            SupervisorName = request.SupervisorName,
            SubmissionDate = newEvent.DueDate,
            ProgressPercentage = request.ProgressPercentage,
            Notes = request.Notes,
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
