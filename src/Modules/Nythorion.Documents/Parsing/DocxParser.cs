using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Nythorion.Documents.Parsing;

public class DocxParser : IDocumentParser
{
    public bool CanParse(string contentType) =>
        contentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public string ExtractText(Stream fileStream)
    {
        using var doc = WordprocessingDocument.Open(fileStream, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null) return string.Empty;

        var sb = new StringBuilder();
        foreach (var paragraph in body.Elements<Paragraph>())
            sb.AppendLine(paragraph.InnerText);

        return sb.ToString();
    }
}
