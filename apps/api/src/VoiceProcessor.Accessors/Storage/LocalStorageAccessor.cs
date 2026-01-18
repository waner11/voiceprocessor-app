using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VoiceProcessor.Accessors.Contracts;

namespace VoiceProcessor.Accessors.Storage;

public class LocalStorageAccessor : IStorageAccessor
{
    private readonly ILogger<LocalStorageAccessor> _logger;
    private readonly LocalStorageOptions _options;

    public LocalStorageAccessor(
        IOptions<LocalStorageOptions> options,
        ILogger<LocalStorageAccessor> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (!Directory.Exists(_options.BasePath))
        {
            Directory.CreateDirectory(_options.BasePath);
        }
    }

    public async Task<string> UploadAsync(
        StorageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(request.Path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(fullPath, request.Data, cancellationToken);

        _logger.LogDebug("Uploaded file to {Path}, size: {Size} bytes",
            request.Path, request.Data.Length);

        return request.Path;
    }

    public async Task<Stream> DownloadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(path);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        var memoryStream = new MemoryStream();
        await using var fileStream = File.OpenRead(fullPath);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<byte[]> DownloadBytesAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(path);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {path}");
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    public Task DeleteAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogDebug("Deleted file at {Path}", path);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(path);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task<string> GetPublicUrlAsync(
        string path,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        // For local storage, return a relative URL that can be served by the API
        var publicUrl = $"{_options.PublicUrlBase.TrimEnd('/')}/{path.TrimStart('/')}";
        return Task.FromResult(publicUrl);
    }

    private string GetFullPath(string path)
    {
        // Prevent path traversal attacks
        var normalizedPath = path.Replace("..", string.Empty).TrimStart('/');
        return Path.Combine(_options.BasePath, normalizedPath);
    }
}

public class LocalStorageOptions
{
    public const string SectionName = "LocalStorage";

    public string BasePath { get; set; } = "./storage";
    public string PublicUrlBase { get; set; } = "/files";
}
