namespace DocAssistant.Api.Models;

/// <summary>
/// A small piece of a document's text. In Phase 2 each chunk gets an embedding
/// stored in Qdrant; PageNumber lets answers cite where the text came from.
/// </summary>
public class DocumentChunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }

    public int Index { get; set; }
    public int PageNumber { get; set; }
    public string Content { get; set; } = string.Empty;
    public int CharCount { get; set; }
}
