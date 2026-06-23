using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.AI.Commands.SummarizeLectureNotes;

public record SummarizeLectureNotesCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long CourseOfferingId,
    long FileSizeInBytes
) : IRequest<long>;

public class SummarizeLectureNotesCommandValidator : AbstractValidator<SummarizeLectureNotesCommand>
{
    public SummarizeLectureNotesCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().WithMessage("File name is required.");
        RuleFor(x => x.ContentType).NotEmpty().WithMessage("Content type is required.");
        RuleFor(x => x.CourseOfferingId).GreaterThan(0).WithMessage("Valid course offering ID is required.");
        RuleFor(x => x.FileSizeInBytes).GreaterThan(0).WithMessage("File cannot be empty.");
    }
}

public class SummarizeLectureNotesCommandHandler : IRequestHandler<SummarizeLectureNotesCommand, long>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IBackgroundJobService _backgroundJobService;

    public SummarizeLectureNotesCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IBlobStorageService _blobStorageService,
        IBackgroundJobService backgroundJobService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        this._blobStorageService = _blobStorageService;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<long> Handle(SummarizeLectureNotesCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = long.Parse(userIdStr);

        // 1. Upload to Blob Storage
        var blobName = $"{Guid.NewGuid()}_{request.FileName}";
        var blobUrl = await _blobStorageService.UploadAsync(
            request.FileStream, 
            blobName, 
            "lecturenotes", 
            request.ContentType, 
            cancellationToken);

        // 2. Persist FileRecord
        var fileRecord = new FileRecord
        {
            UploadedByUserId = userId,
            CourseOfferingId = request.CourseOfferingId,
            FileName = request.FileName,
            BlobStorageUrl = blobUrl,
            BlobContainer = "lecturenotes",
            FileSizeInBytes = request.FileSizeInBytes,
            MimeType = request.ContentType,
            Category = "LectureNote",
            UploadedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<FileRecord>().AddAsync(fileRecord, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Queue Background summarization
        _backgroundJobService.Enqueue<ISender>(sender => 
            sender.Send(new ProcessLectureSummaryCommand(fileRecord.Id), CancellationToken.None));

        return fileRecord.Id;
    }
}
