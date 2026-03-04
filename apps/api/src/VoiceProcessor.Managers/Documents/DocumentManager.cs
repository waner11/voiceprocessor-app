using Microsoft.Extensions.Logging;
using VoiceProcessor.Accessors.Documents;
using VoiceProcessor.Domain.DTOs.Documents;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Managers.Documents;

public class DocumentManager : IDocumentManager
{
    private const long MaxFileSizeBytes = 50L * 1024 * 1024;

    private readonly IDocumentParserAccessor _documentParserAccessor;
    private readonly ILogger<DocumentManager> _logger;

    public DocumentManager(
        IDocumentParserAccessor documentParserAccessor,
        ILogger<DocumentManager> logger)
    {
        _documentParserAccessor = documentParserAccessor;
        _logger = logger;
    }

    public Task<DocumentExtractionResult> ExtractTextAsync(
        Stream fileStream,
        string mimeType,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (fileStream.CanSeek && fileStream.Length > MaxFileSizeBytes)
        {
            throw new FileTooLargeException("File exceeds the 50MB limit.");
        }

        _logger.LogInformation(
            "Document extraction requested for file {FileName} ({MimeType})",
            fileName,
            mimeType);

        return _documentParserAccessor.ExtractTextAsync(fileStream, mimeType, fileName);
    }
}
