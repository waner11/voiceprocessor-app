namespace VoiceProcessor.Accessors.Documents;
public class DocumentParsingException : InvalidOperationException
{
    public DocumentParsingException(string message) : base(message)
    {
    }
}

public class FileTooLargeException : DocumentParsingException
{
    public FileTooLargeException(string message) : base(message)
    {
    }
}
