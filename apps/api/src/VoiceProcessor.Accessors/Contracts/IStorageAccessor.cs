namespace VoiceProcessor.Accessors.Contracts;

public interface IStorageAccessor
{
    Task<string> UploadAsync(
        StorageUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Stream> DownloadAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task<byte[]> DownloadBytesAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string path,
        CancellationToken cancellationToken = default);

    Task<string> GetPublicUrlAsync(
        string path,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default);
}

public record StorageUploadRequest
{
    public required byte[] Data { get; init; }
    public required string Path { get; init; }
    public required string ContentType { get; init; }
    public IDictionary<string, string>? Metadata { get; init; }
}
