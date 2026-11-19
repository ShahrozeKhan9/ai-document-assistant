using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace DocAssistant.Api.Services;

public class PdfTextExtractor : IPdfTextExtractor
{
    public IReadOnlyList<PageText> Extract(Stream pdfStream)
    {
        var pages = new List<PageText>();

        using var pdf = PdfDocument.Open(pdfStream);
        foreach (var page in pdf.GetPages())
        {
            // ContentOrderTextExtractor keeps words in reading order,
            // which gives cleaner chunks than page.Text.
            var text = ContentOrderTextExtractor.GetText(page);
            pages.Add(new PageText(page.Number, text));
        }

        return pages;
    }
}
