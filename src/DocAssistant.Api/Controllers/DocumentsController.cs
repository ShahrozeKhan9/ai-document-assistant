using DocAssistant.Api.DTOs;
using DocAssistant.Api.Options;
using DocAssistant.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DocAssistant.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documents;
    private readonly StorageOptions _storage;

    public DocumentsController(IDocumentService documents, IOptions<StorageOptions> storage)
    {
        _documents = documents;
        _storage = storage.Value;
    }

    /// <summary>Upload a PDF. It is split into chunks right away.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(DocumentDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return Problem("Please choose a file to upload.", statusCode: 400);

        if (!Path.GetExtension(file.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            return Problem("Only PDF files are supported.", statusCode: 400);

        var maxBytes = _storage.MaxFileSizeMb * 1024L * 1024L;
        if (file.Length > maxBytes)
            return Problem($"File is larger than {_storage.MaxFileSizeMb} MB.", statusCode: 400);

        var result = await _documents.UploadAsync(file, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    /// <summary>List all uploaded documents, newest first.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DocumentSummaryDto>>> List(CancellationToken ct) =>
        Ok(await _documents.ListAsync(ct));

    /// <summary>Get one document's details.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DocumentDetailDto>> Get(Guid id, CancellationToken ct)
    {
        var doc = await _documents.GetAsync(id, ct);
        return doc is null ? NotFound() : Ok(doc);
    }

    /// <summary>See the chunks a document was split into (useful for checking chunk quality).</summary>
    [HttpGet("{id:guid}/chunks")]
    public async Task<ActionResult<IReadOnlyList<ChunkDto>>> GetChunks(Guid id, CancellationToken ct)
    {
        var chunks = await _documents.GetChunksAsync(id, ct);
        return chunks is null ? NotFound() : Ok(chunks);
    }

    /// <summary>Delete a document, its chunks and the stored file.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await _documents.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
