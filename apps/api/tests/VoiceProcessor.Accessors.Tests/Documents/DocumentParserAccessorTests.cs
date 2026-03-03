using FluentAssertions;
using VoiceProcessor.Accessors.Documents;

namespace VoiceProcessor.Accessors.Tests.Documents;

public class DocumentParserAccessorTests
{
    private static readonly string TestDataPath = Path.Combine(
        AppContext.BaseDirectory,
        "TestData");

    [Fact]
    public async Task PdfDocumentParserAccessor_ExtractTextAsync_ExtractsTextFromAllPages()
    {
        await using var stream = File.OpenRead(Path.Combine(TestDataPath, "sample.pdf"));
        var accessor = new PdfDocumentParserAccessor();

        var result = await accessor.ExtractTextAsync(stream, "application/pdf", "sample.pdf");

        result.PageCount.Should().Be(2);
        result.Text.Should().Be("PDF page one content.\n\nPDF page two content.");
        result.WordCount.Should().Be(8);
        result.CharacterCount.Should().Be(result.Text.Length);
    }

    [Fact]
    public async Task DocxDocumentParserAccessor_ExtractTextAsync_ExtractsBodyParagraphsOnly()
    {
        await using var stream = File.OpenRead(Path.Combine(TestDataPath, "sample.docx"));
        var accessor = new DocxDocumentParserAccessor();

        var result = await accessor.ExtractTextAsync(stream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "sample.docx");

        result.PageCount.Should().BeNull();
        result.Text.Should().Be("Docx paragraph one.\n\nDocx paragraph two.");
        result.WordCount.Should().Be(6);
        result.CharacterCount.Should().Be(result.Text.Length);
    }

    [Fact]
    public async Task DocumentParserAccessor_ExtractTextAsync_WithUnsupportedMimeType_ThrowsNotSupportedException()
    {
        await using var stream = new MemoryStream([1, 2, 3]);
        var accessor = new DocumentParserAccessor([
            new PdfDocumentParserAccessor(),
            new DocxDocumentParserAccessor()
        ]);

        var act = async () => await accessor.ExtractTextAsync(stream, "text/plain", "sample.txt");

        await act.Should().ThrowAsync<NotSupportedException>()
            .WithMessage("*text/plain*");
    }

    [Fact]
    public async Task DocumentParserAccessor_ExtractTextAsync_WhenFileExceeds50Mb_ThrowsInvalidOperationException()
    {
        await using var stream = new MemoryStream(new byte[50 * 1024 * 1024 + 1]);
        var accessor = new DocumentParserAccessor([
            new PdfDocumentParserAccessor(),
            new DocxDocumentParserAccessor()
        ]);

        var act = async () => await accessor.ExtractTextAsync(stream, "application/pdf", "large.pdf");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*50MB*");
    }
}
