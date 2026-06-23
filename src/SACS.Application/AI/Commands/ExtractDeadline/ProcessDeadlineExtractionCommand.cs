using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SACS.Application.Common.Interfaces;
using SACS.Domain.Entities;
using SACS.Domain.Repositories;

namespace SACS.Application.AI.Commands.ExtractDeadline;

public record ProcessDeadlineExtractionCommand(long IngestedMessageId) : IRequest;

public class ProcessDeadlineExtractionCommandHandler : IRequestHandler<ProcessDeadlineExtractionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiServiceClient _aiServiceClient;

    public ProcessDeadlineExtractionCommandHandler(IUnitOfWork unitOfWork, IAiServiceClient aiServiceClient)
    {
        _unitOfWork = unitOfWork;
        _aiServiceClient = aiServiceClient;
    }

    public async Task Handle(ProcessDeadlineExtractionCommand request, CancellationToken cancellationToken)
    {
        var message = await _unitOfWork.Repository<IngestedMessage>().GetByIdAsync(request.IngestedMessageId, cancellationToken);
        if (message == null)
        {
            return;
        }

        message.ProcessingStatus = "Processing";
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var response = await _aiServiceClient.ExtractDeadlinesAsync(message.RawContent, cancellationToken);

            foreach (var item in response.Deadlines)
            {
                var extractedDeadline = new ExtractedDeadline
                {
                    IngestedMessageId = message.Id,
                    Title = item.Title,
                    CourseCodeGuess = item.CourseCodeGuess,
                    ParsedDueDate = item.ParsedDueDate,
                    ConfidenceScore = item.ConfidenceScore,
                    IsConfirmed = false,
                    IsRejected = false,
                    ExtractedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<ExtractedDeadline>().AddAsync(extractedDeadline, cancellationToken);
            }

            message.ProcessingStatus = "Completed";
            message.ProcessedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            message.ProcessingStatus = "Failed";
            message.ErrorMessage = ex.Message;
            message.ProcessedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
