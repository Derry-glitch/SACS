namespace SACS.Application.Common.Events;

public class DeadlineExtractionEvent
{
    public long IngestedMessageId { get; set; }

    public DeadlineExtractionEvent() { }

    public DeadlineExtractionEvent(long ingestedMessageId)
    {
        IngestedMessageId = ingestedMessageId;
    }
}
