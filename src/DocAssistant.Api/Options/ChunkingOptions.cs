namespace DocAssistant.Api.Options;

public class ChunkingOptions
{
    public const string SectionName = "Chunking";

    /// <summary>Target characters per chunk.</summary>
    public int ChunkSize { get; set; } = 1000;

    /// <summary>Characters repeated between neighbouring chunks so context isn't cut mid-thought.</summary>
    public int ChunkOverlap { get; set; } = 200;
}
