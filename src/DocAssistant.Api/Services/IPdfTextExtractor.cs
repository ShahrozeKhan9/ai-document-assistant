namespace DocAssistant.Api.Services;

public record PageText(int PageNumber, string Text);

public interface IPdfTextExtractor
{
    /// <summary>Reads a PDF and returns the text of each page, in order.</summary>
    IReadOnlyList<PageText> Extract(Stream pdfStream);
}
