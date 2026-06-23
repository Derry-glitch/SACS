namespace SACS.Application.Common.Events;

public class LectureNoteSummarizationEvent
{
    public long FileRecordId { get; set; }

    public LectureNoteSummarizationEvent() { }

    public LectureNoteSummarizationEvent(long fileRecordId)
    {
        FileRecordId = fileRecordId;
    }
}
