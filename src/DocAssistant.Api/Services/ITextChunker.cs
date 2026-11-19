namespace DocAssistant.Api.Services;

public interface ITextChunker
{
    /// <summary>Splits text into overlapping chunks, preferring sentence or word boundaries.</summary>
    IReadOnlyList<string> Chunk(string text);
}
