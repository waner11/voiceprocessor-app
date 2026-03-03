using System.Net;

namespace VoiceProcessor.Accessors.Documents;

public class DocumentParsingException : InvalidOperationException
{
    public DocumentParsingException(string message) : base(message)
    {
    }

    public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.BadRequest;
}
