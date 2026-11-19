using DocAssistant.Api.Models;

namespace DocAssistant.Api.DTOs;

public record DocumentSummaryDto(
    Guid Id,
    string FileName,
    long SizeBytes,
    int PageCount,
    int ChunkCount,
    DocumentStatus Status,
    DateTime UploadedAtUtc);

public record DocumentDetailDto(
    Guid Id,
    string FileName,
    long SizeBytes,
    int PageCount,
    int ChunkCount,
    DocumentStatus Status,
    string? Error,
    DateTime UploadedAtUtc);

public record ChunkDto(
    int Index,
    int PageNumber,
    int CharCount,
    string Content);
