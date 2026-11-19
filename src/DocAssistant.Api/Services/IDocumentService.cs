using DocAssistant.Api.DTOs;

namespace DocAssistant.Api.Services;

public interface IDocumentService
{
    Task<DocumentDetailDto> UploadAsync(IFormFile file, CancellationToken ct);
    Task<IReadOnlyList<DocumentSummaryDto>> ListAsync(CancellationToken ct);
    Task<DocumentDetailDto?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ChunkDto>?> GetChunksAsync(Guid id, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
