using System.Text;
using UglyToad.PdfPig;

namespace Nythorion.Documents.Parsing;

public class PdfParser : IDocumentParser
{
    public bool CanParse(string contentType) => contentType == "application/pdf";

    public string ExtractText(Stream fileStream)
    {
        using var document = PdfDocument.Open(fileStream);
        var sb = new StringBuilder();

        foreach (var page in document.GetPages())
            sb.AppendLine(string.Join(" ", page.GetWords().Select(w => w.Text)));

        return sb.ToString();
    }
}
