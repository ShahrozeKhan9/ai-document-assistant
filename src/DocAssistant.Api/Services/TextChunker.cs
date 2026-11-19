using System.Text.RegularExpressions;
using DocAssistant.Api.Options;
using Microsoft.Extensions.Options;

namespace DocAssistant.Api.Services;

public class TextChunker : ITextChunker
{
    private static readonly char[] SentenceEnds = { '.', '!', '?' };
    private readonly int _size;
    private readonly int _overlap;

    public TextChunker(IOptions<ChunkingOptions> options)
    {
        _size = Math.Max(100, options.Value.ChunkSize);
        // Overlap must stay smaller than the chunk, otherwise we'd never move forward.
        _overlap = Math.Clamp(options.Value.ChunkOverlap, 0, _size / 2);
    }

    public IReadOnlyList<string> Chunk(string text)
    {
        var chunks = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return chunks;

        // Collapse line breaks, tabs and repeated spaces from the PDF into single spaces.
        text = Regex.Replace(text, @"\s+", " ").Trim();

        var start = 0;
        while (start < text.Length)
        {
            var end = Math.Min(start + _size, text.Length);

            if (end < text.Length)
            {
                var window = end - start;
                var minEnd = start + _size / 2; // don't make chunks too small

                // 1) Prefer ending at the last sentence end inside the window.
                var sentenceEnd = text.LastIndexOfAny(SentenceEnds, end - 1, window);
                if (sentenceEnd >= minEnd)
                {
                    end = sentenceEnd + 1;
                }
                else
                {
                    // 2) Otherwise end at the last space, so we don't cut a word.
                    var space = text.LastIndexOf(' ', end - 1, window);
                    if (space >= minEnd) end = space;
                }
            }

            var chunk = text[start..end].Trim();
            if (chunk.Length > 0) chunks.Add(chunk);

            if (end >= text.Length) break;

            // Step back by the overlap, then move forward to the start of a word.
            var next = Math.Max(end - _overlap, start + 1);
            while (next < end && text[next - 1] != ' ') next++;
            start = next;
        }

        return chunks;
    }
}
