namespace VoiceProcessor.Managers.Contracts;

public interface IGenerationProcessor
{
    Task ProcessGenerationAsync(Guid generationId, CancellationToken cancellationToken = default);
}
