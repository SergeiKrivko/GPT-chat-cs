namespace Core.Search;

public class Searcher
{
    public static async Task<List<SearchResult>> Search(Guid chatId, string? text)
    {
        if (string.IsNullOrEmpty(text))
            return new();
        var messages = await LocalRepository.LocalRepository.Instance.GetAllMessages(chatId, selectReplys: false);
        var result = new List<SearchResult>();

        foreach (var message in messages)
        {
            var offset = 0;
            while ((offset = message.Text.IndexOf(text, offset, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                result.Add(new SearchResult(message.Id, offset, text.Length));
                offset += 1;
            }
        }

        return result;
    }
}