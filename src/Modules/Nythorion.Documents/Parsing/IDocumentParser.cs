namespace Nythorion.Documents.Parsing;

public interface IDocumentParser
{
    bool CanParse(string contentType);
    string ExtractText(Stream fileStream);
}
