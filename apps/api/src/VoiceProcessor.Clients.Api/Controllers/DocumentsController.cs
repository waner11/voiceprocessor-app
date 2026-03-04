using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoiceProcessor.Accessors.Documents;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Clients.Api.Controllers;

public class DocumentsController : ApiControllerBase
{
    private readonly IDocumentManager _documentManager;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        IDocumentManager documentManager,
        ILogger<DocumentsController> logger)
    {
        _documentManager = documentManager;
        _logger = logger;
    }

    /// <summary>
    /// Extract text from an uploaded document (PDF or DOCX).
    /// </summary>
    [HttpPost("extract")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(DocumentExtractionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status413RequestEntityTooLarge)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<DocumentExtractionResponse>> ExtractText(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new ErrorResponse
            {
                Code = "NO_FILE",
                Message = "A non-empty file must be provided."
            });
        }

        _logger.LogInformation(
            "Document extraction requested for file {FileName} ({ContentType}, {Size} bytes)",
            file.FileName, file.ContentType, file.Length);

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _documentManager.ExtractTextAsync(
                stream,
                file.ContentType,
                file.FileName,
                cancellationToken);

            _logger.LogInformation(
                "Document extraction succeeded for {FileName}: {WordCount} words, {CharCount} chars",
                file.FileName, result.WordCount, result.CharacterCount);

            return Ok(new DocumentExtractionResponse(
                result.Text,
                result.PageCount,
                result.WordCount,
                result.CharacterCount));
        }
        catch (DocumentParsingException ex) when (ex.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge)
        {
            _logger.LogWarning("Document extraction rejected — file too large: {FileName}", file.FileName);
            return StatusCode(StatusCodes.Status413RequestEntityTooLarge,
                new ErrorResponse { Code = "FILE_TOO_LARGE", Message = ex.Message });
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning("Document extraction rejected — unsupported format: {FileName} ({ContentType})",
                file.FileName, file.ContentType);
            return StatusCode(StatusCodes.Status415UnsupportedMediaType,
                new ErrorResponse { Code = "UNSUPPORTED_FORMAT", Message = ex.Message });
        }
    }
}
