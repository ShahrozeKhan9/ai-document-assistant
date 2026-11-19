namespace DocAssistant.Api.Models;

public enum DocumentStatus
{
    Processing,
    Ready,
    Failed
}

public class Document
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int PageCount { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Processing;
    public string? Error { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    public List<DocumentChunk> Chunks { get; set; } = new();
}
