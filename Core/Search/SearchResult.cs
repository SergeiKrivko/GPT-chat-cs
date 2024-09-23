namespace Core.Search;

public class SearchResult
{
    public Guid MessageId { get; }
    public int Offset { get; }
    public int Len { get; }
    public bool Selected { get; set; } = false;

    public SearchResult(Guid messageId, int offset, int len)
    {
        MessageId = messageId;
        Offset = offset;
        Len = len;
    }
}