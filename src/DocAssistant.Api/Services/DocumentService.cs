using DocAssistant.Api.Data;
using DocAssistant.Api.DTOs;
using DocAssistant.Api.Models;
using DocAssistant.Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DocAssistant.Api.Services;

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    private readonly IPdfTextExtractor _extractor;
    private readonly ITextChunker _chunker;
    private readonly ILogger<DocumentService> _logger;
    private readonly string _storageRoot;

    public DocumentService(
        AppDbContext db,
        IPdfTextExtractor extractor,
        ITextChunker chunker,
        IOptions<StorageOptions> storage,
        IWebHostEnvironment env,
        ILogger<DocumentService> logger)
    {
        _db = db;
        _extractor = extractor;
        _chunker = chunker;
        _logger = logger;
        _storageRoot = Path.Combine(env.ContentRootPath, storage.Value.RootPath);
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<DocumentDetailDto> UploadAsync(IFormFile file, CancellationToken ct)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            FileName = Path.GetFileName(file.FileName),
            SizeBytes = file.Length,
            Status = DocumentStatus.Processing
        };
        document.StoredFileName = $"{document.Id}.pdf";

        // 1) Keep a copy of the original file (needed later for re-processing).
        var path = Path.Combine(_storageRoot, document.StoredFileName);
        await using (var fs = File.Create(path))
        {
            await file.CopyToAsync(fs, ct);
        }

        // 2) Extract text page by page and split each page into chunks.
        try
        {
            await using var stream = File.OpenRead(path);
            var pages = _extractor.Extract(stream);
            document.PageCount = pages.Count;

            var index = 0;
            foreach (var page in pages)
            {
                foreach (var piece in _chunker.Chunk(page.Text))
                {
                    document.Chunks.Add(new DocumentChunk
                    {
                        Id = Guid.NewGuid(),
                        Index = index++,
                        PageNumber = page.PageNumber,
                        Content = piece,
                        CharCount = piece.Length
                    });
                }
            }

            if (document.Chunks.Count == 0)
            {
                document.Status = DocumentStatus.Failed;
                document.Error = "No text found. The PDF may be scanned images (OCR is not supported yet).";
            }
            else
            {
                document.Status = DocumentStatus.Ready;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process {FileName}", document.FileName);
            document.Status = DocumentStatus.Failed;
            document.Error = "Could not read this PDF. It may be corrupted or password-protected.";
            document.Chunks.Clear();
        }

        // 3) Save the document and its chunks in one transaction.
        _db.Documents.Add(document);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Uploaded {FileName}: {Pages} pages, {Chunks} chunks, status {Status}",
            document.FileName, document.PageCount, document.Chunks.Count, document.Status);

        return ToDetail(document, document.Chunks.Count);
    }

    public async Task<IReadOnlyList<DocumentSummaryDto>> ListAsync(CancellationToken ct) =>
        await _db.Documents
            .AsNoTracking()
            .OrderByDescending(d => d.UploadedAtUtc)
            .Select(d => new DocumentSummaryDto(
                d.Id, d.FileName, d.SizeBytes, d.PageCount, d.Chunks.Count, d.Status, d.UploadedAtUtc))
            .ToListAsync(ct);

    public async Task<DocumentDetailDto?> GetAsync(Guid id, CancellationToken ct) =>
        await _db.Documents
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DocumentDetailDto(
                d.Id, d.FileName, d.SizeBytes, d.PageCount, d.Chunks.Count, d.Status, d.Error, d.UploadedAtUtc))
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ChunkDto>?> GetChunksAsync(Guid id, CancellationToken ct)
    {
        var exists = await _db.Documents.AnyAsync(d => d.Id == id, ct);
        if (!exists) return null;

        return await _db.DocumentChunks
            .AsNoTracking()
            .Where(c => c.DocumentId == id)
            .OrderBy(c => c.Index)
            .Select(c => new ChunkDto(c.Index, c.PageNumber, c.CharCount, c.Content))
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var document = await _db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (document is null) return false;

        _db.Documents.Remove(document); // chunks are removed by cascade delete
        await _db.SaveChangesAsync(ct);

        var path = Path.Combine(_storageRoot, document.StoredFileName);
        if (File.Exists(path)) File.Delete(path);

        return true;
    }

    private static DocumentDetailDto ToDetail(Document d, int chunkCount) =>
        new(d.Id, d.FileName, d.SizeBytes, d.PageCount, chunkCount, d.Status, d.Error, d.UploadedAtUtc);
}
