using VoiceProcessor.Domain.DTOs.Documents;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using VoiceProcessor.Accessors.Documents;
using VoiceProcessor.Clients.Api.Controllers;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Managers.Contracts;

namespace VoiceProcessor.Managers.Tests.Documents;

public class DocumentsControllerTests
{
    private readonly Mock<IDocumentManager> _mockDocumentManager;
    private readonly Mock<ILogger<DocumentsController>> _mockLogger;

    public DocumentsControllerTests()
    {
        _mockDocumentManager = new Mock<IDocumentManager>();
        _mockLogger = new Mock<ILogger<DocumentsController>>();
    }

    private DocumentsController CreateController()
    {
        return new DocumentsController(_mockDocumentManager.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExtractText_WithValidPdf_ReturnsExtractionResult()
    {
        // Arrange
        var controller = CreateController();
        var extractionResult = new DocumentExtractionResult(
            Text: "Hello world from PDF",
            PageCount: 2,
            WordCount: 4,
            CharacterCount: 20);

        _mockDocumentManager
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), "application/pdf", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractionResult);

        var fileContent = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF magic bytes
        var formFile = new FormFile(
            new MemoryStream(fileContent),
            baseStreamOffset: 0,
            length: fileContent.Length,
            name: "file",
            fileName: "test.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        // Act
        var result = await controller.ExtractText(formFile, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<DocumentExtractionResponse>().Subject;
        response.Text.Should().Be("Hello world from PDF");
        response.PageCount.Should().Be(2);
        response.WordCount.Should().Be(4);
        response.CharacterCount.Should().Be(20);
    }

    [Fact]
    public async Task ExtractText_WithValidDocx_ReturnsExtractionResult()
    {
        // Arrange
        var controller = CreateController();
        var extractionResult = new DocumentExtractionResult(
            Text: "Hello world from DOCX",
            PageCount: null,
            WordCount: 4,
            CharacterCount: 21);

        _mockDocumentManager
            .Setup(x => x.ExtractTextAsync(
                It.IsAny<Stream>(),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractionResult);

        var fileContent = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // ZIP/DOCX magic bytes
        var formFile = new FormFile(
            new MemoryStream(fileContent),
            baseStreamOffset: 0,
            length: fileContent.Length,
            name: "file",
            fileName: "test.docx")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        // Act
        var result = await controller.ExtractText(formFile, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<DocumentExtractionResponse>().Subject;
        response.Text.Should().Be("Hello world from DOCX");
        response.PageCount.Should().BeNull();
        response.WordCount.Should().Be(4);
        response.CharacterCount.Should().Be(21);
    }

    [Fact]
    public async Task ExtractText_WithNoFile_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.ExtractText(null!, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ExtractText_WithUnsupportedMimeType_Returns415()
    {
        // Arrange
        var controller = CreateController();

        _mockDocumentManager
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), "text/plain", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotSupportedException("No document parser is configured for mime type 'text/plain'"));

        var fileContent = new byte[] { 0x48, 0x65, 0x6C, 0x6C }; // "Hell"
        var formFile = new FormFile(
            new MemoryStream(fileContent),
            baseStreamOffset: 0,
            length: fileContent.Length,
            name: "file",
            fileName: "test.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        // Act
        var result = await controller.ExtractText(formFile, CancellationToken.None);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status415UnsupportedMediaType);
    }

    [Fact]
    public async Task ExtractText_WithFileTooLarge_Returns413()
    {
        // Arrange
        var controller = CreateController();

        _mockDocumentManager
            .Setup(x => x.ExtractTextAsync(It.IsAny<Stream>(), "application/pdf", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DocumentParsingException("File exceeds the 50MB limit.")
            {
                StatusCode = System.Net.HttpStatusCode.RequestEntityTooLarge
            });

        var fileContent = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var formFile = new FormFile(
            new MemoryStream(fileContent),
            baseStreamOffset: 0,
            length: fileContent.Length,
            name: "file",
            fileName: "large.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        // Act
        var result = await controller.ExtractText(formFile, CancellationToken.None);

        // Assert
        var statusResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(StatusCodes.Status413RequestEntityTooLarge);
    }

    [Fact]
    public async Task ExtractText_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var controller = CreateController();

        var formFile = new FormFile(
            new MemoryStream(Array.Empty<byte>()),
            baseStreamOffset: 0,
            length: 0,
            name: "file",
            fileName: "empty.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        // Act
        var result = await controller.ExtractText(formFile, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
