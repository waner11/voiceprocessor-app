using Microsoft.Extensions.Logging;
using VoiceProcessor.Accessors.Contracts;
using VoiceProcessor.Accessors.Providers;
using VoiceProcessor.Domain.Entities;
using VoiceProcessor.Domain.Enums;
using VoiceProcessor.Engines.Contracts;
using VoiceProcessor.Managers.Contracts;
using VoiceProcessor.Utilities.Timing;

namespace VoiceProcessor.Managers.Generation;

public class GenerationProcessor : IGenerationProcessor
{
    private const int MaxChunkRetries = 3;
    private static readonly TimeSpan[] RetryDelays = [
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(10)
    ];

    private readonly IGenerationAccessor _generationAccessor;
    private readonly IGenerationChunkAccessor _chunkAccessor;
    private readonly IVoiceAccessor _voiceAccessor;
    private readonly IUserAccessor _userAccessor;
    private readonly ITtsProviderFactory _providerFactory;
    private readonly IStorageAccessor _storageAccessor;
    private readonly IChunkingEngine _chunkingEngine;
    private readonly IAudioMergeEngine _audioMergeEngine;
    private readonly ILogger<GenerationProcessor> _logger;
    private readonly IDelayService _delayService;

    public GenerationProcessor(
        IGenerationAccessor generationAccessor,
        IGenerationChunkAccessor chunkAccessor,
        IVoiceAccessor voiceAccessor,
        IUserAccessor userAccessor,
        ITtsProviderFactory providerFactory,
        IStorageAccessor storageAccessor,
        IChunkingEngine chunkingEngine,
        IAudioMergeEngine audioMergeEngine,
        ILogger<GenerationProcessor> logger,
        IDelayService delayService)
    {
        _generationAccessor = generationAccessor;
        _chunkAccessor = chunkAccessor;
        _voiceAccessor = voiceAccessor;
        _userAccessor = userAccessor;
        _providerFactory = providerFactory;
        _storageAccessor = storageAccessor;
        _chunkingEngine = chunkingEngine;
        _audioMergeEngine = audioMergeEngine;
        _logger = logger;
        _delayService = delayService;
    }

    public async Task ProcessGenerationAsync(Guid generationId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting generation processing for {GenerationId}", generationId);

        var generation = await _generationAccessor.GetByIdAsync(generationId, cancellationToken);
        if (generation is null)
        {
            _logger.LogError("Generation {GenerationId} not found", generationId);
            return;
        }

        if (generation.Status is GenerationStatus.Completed or GenerationStatus.Failed or GenerationStatus.Cancelled)
        {
            _logger.LogWarning("Generation {GenerationId} already in terminal state: {Status}",
                generationId, generation.Status);
            return;
        }

        try
        {
            // Update status to processing
            await _generationAccessor.UpdateStatusAsync(
                generationId, GenerationStatus.Processing, cancellationToken: cancellationToken);

            // Get voice details
            var voice = await _voiceAccessor.GetByIdAsync(generation.VoiceId, cancellationToken);
            if (voice is null)
            {
                throw new InvalidOperationException($"Voice {generation.VoiceId} not found");
            }

            // Get TTS provider
            var provider = _providerFactory.GetProvider(voice.Provider);

            // Split text into chunks
            var textChunks = _chunkingEngine.SplitText(generation.InputText);
            _logger.LogInformation("Generation {GenerationId} split into {ChunkCount} chunks",
                generationId, textChunks.Count);

            // Process each chunk
            var audioChunks = new List<byte[]>();
            decimal totalCost = 0;
            var totalDurationMs = 0;
            var chunksCompleted = 0;

            for (var i = 0; i < textChunks.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var textChunk = textChunks[i];
                _logger.LogDebug("Processing chunk {ChunkIndex}/{ChunkCount} for generation {GenerationId}",
                    i + 1, textChunks.Count, generationId);

                // Create chunk record
                var chunk = new GenerationChunk
                {
                    Id = Guid.NewGuid(),
                    GenerationId = generationId,
                    Index = i,
                    Text = textChunk.Text,
                    CharacterCount = textChunk.CharacterCount,
                    Status = ChunkStatus.Processing,
                    Provider = voice.Provider,
                    CreatedAt = DateTime.UtcNow
                };
                await _chunkAccessor.CreateAsync(chunk, cancellationToken);

                // Retry loop for transient failures
                Exception? lastException = null;
                for (var attempt = 0; attempt <= MaxChunkRetries; attempt++)
                {
                    try
                    {
                        if (attempt > 0)
                        {
                            chunk.RetryCount = attempt;
                            _logger.LogInformation(
                                "Retrying chunk {ChunkIndex} for generation {GenerationId}, attempt {Attempt}",
                                i, generationId, attempt);
                            await _delayService.DelayAsync(RetryDelays[attempt - 1], cancellationToken);
                        }

                        // Call TTS provider
                        var result = await provider.GenerateSpeechAsync(new TtsRequest
                        {
                            Text = textChunk.Text,
                            ProviderVoiceId = voice.ProviderVoiceId,
                            OutputFormat = generation.AudioFormat ?? "mp3"
                        }, cancellationToken);

                        if (!result.Success || result.AudioData is null)
                        {
                            throw new InvalidOperationException(
                                result.ErrorMessage ?? "TTS generation failed");
                        }

                        // Store chunk audio
                        var chunkPath = $"generations/{generationId}/chunks/{i}.{generation.AudioFormat ?? "mp3"}";
                        var chunkUrl = await _storageAccessor.UploadAsync(new StorageUploadRequest
                        {
                            Data = result.AudioData,
                            Path = chunkPath,
                            ContentType = result.ContentType ?? "audio/mpeg"
                        }, cancellationToken);

                        // Update chunk status
                        chunk.Status = ChunkStatus.Completed;
                        chunk.AudioUrl = chunkUrl;
                        chunk.Cost = result.Cost;
                        chunk.AudioDurationMs = result.DurationMs;
                        chunk.CompletedAt = DateTime.UtcNow;
                        await _chunkAccessor.UpdateAsync(chunk, cancellationToken);

                        audioChunks.Add(result.AudioData);
                        totalCost += result.Cost;
                        totalDurationMs += result.DurationMs ?? 0;
                        chunksCompleted++;

                        // Update generation progress
                        var progress = (int)((chunksCompleted / (double)textChunks.Count) * 100);
                        await _generationAccessor.UpdateProgressAsync(
                            generationId, chunksCompleted, progress, cancellationToken);

                        lastException = null;
                        break; // Success, exit retry loop
                    }
                    catch (OperationCanceledException)
                    {
                        throw; // Don't retry on cancellation
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        _logger.LogWarning(ex,
                            "Chunk {ChunkIndex} for generation {GenerationId} failed on attempt {Attempt}",
                            i, generationId, attempt + 1);

                        if (attempt == MaxChunkRetries)
                        {
                            _logger.LogError(ex,
                                "Failed to process chunk {ChunkIndex} for generation {GenerationId} after {MaxRetries} retries",
                                i, generationId, MaxChunkRetries);

                            chunk.Status = ChunkStatus.Failed;
                            chunk.ErrorMessage = ex.Message;
                            await _chunkAccessor.UpdateAsync(chunk, cancellationToken);
                        }
                    }
                }

                if (lastException is not null)
                {
                    throw lastException;
                }
            }

            // Merge audio chunks using AudioMergeEngine
            var mergeResult = await _audioMergeEngine.MergeAudioChunksAsync(
                audioChunks,
                new AudioMergeOptions
                {
                    OutputFormat = generation.AudioFormat ?? "mp3"
                },
                cancellationToken);

            // Store final audio
            var finalPath = $"generations/{generationId}/audio.{generation.AudioFormat ?? "mp3"}";
            var audioUrl = await _storageAccessor.UploadAsync(new StorageUploadRequest
            {
                Data = mergeResult.AudioData,
                Path = finalPath,
                ContentType = mergeResult.ContentType
            }, cancellationToken);

            // Update generation as completed
            await _generationAccessor.SetCompletedAsync(
                generationId,
                audioUrl,
                generation.AudioFormat ?? "mp3",
                mergeResult.DurationMs,
                mergeResult.SizeBytes,
                totalCost,
                cancellationToken);

            _logger.LogInformation(
                "Generation {GenerationId} completed successfully. Cost: {Cost}, Chunks: {Chunks}",
                generationId, totalCost, chunksCompleted);

            // Deduct credits from user (isolated — must not affect generation status)
            var creditsToDeduct = (int)Math.Ceiling(totalCost * 100);
            for (var creditAttempt = 0; creditAttempt <= MaxChunkRetries; creditAttempt++)
            {
                try
                {
                    if (creditAttempt > 0)
                    {
                        _logger.LogWarning(
                            "Retrying credit deduction for generation {GenerationId}, attempt {Attempt}/{MaxAttempts}, credits: {Credits}",
                            generationId, creditAttempt, MaxChunkRetries, creditsToDeduct);
                        await _delayService.DelayAsync(RetryDelays[creditAttempt - 1], CancellationToken.None);
                    }

                    await _userAccessor.DeductCreditsAsync(
                        generation.UserId,
                        creditsToDeduct,
                        CancellationToken.None);

                    break; // Success
                }
                catch (Exception ex)
                {
                    if (creditAttempt == MaxChunkRetries)
                    {
                        _logger.LogCritical(ex,
                            "BILLING: Failed to deduct {Credits} credits from user {UserId} for completed generation {GenerationId} after {MaxAttempts} attempts. Manual reconciliation required.",
                            creditsToDeduct, generation.UserId, generationId, MaxChunkRetries + 1);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Generation {GenerationId} was cancelled", generationId);
            await _generationAccessor.UpdateStatusAsync(
                generationId, GenerationStatus.Cancelled, cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generation {GenerationId} failed", generationId);
            await _generationAccessor.UpdateStatusAsync(
                generationId,
                GenerationStatus.Failed,
                errorMessage: ex.Message,
                cancellationToken: CancellationToken.None);
        }
    }
}
