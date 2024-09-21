using SQLite;

namespace Core.LocalRepository.Models;

public class TranslationLocalModel
{
    [PrimaryKey] public Guid MessageId { get; set; }
    public string Text { get; set; }
    public string SrcLang { get; set; }
    public string DstLang { get; set; }
}