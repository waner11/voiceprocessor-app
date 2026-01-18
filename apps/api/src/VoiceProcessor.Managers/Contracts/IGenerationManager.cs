using VoiceProcessor.Domain.DTOs.Requests;
using VoiceProcessor.Domain.DTOs.Responses;
using VoiceProcessor.Domain.Enums;

namespace VoiceProcessor.Managers.Contracts;

public interface IGenerationManager
{
    Task<CostEstimateResponse> EstimateCostAsync(
        EstimateCostRequest request,
        CancellationToken cancellationToken = default);

    Task<GenerationResponse> CreateGenerationAsync(
        Guid userId,
        CreateGenerationRequest request,
        CancellationToken cancellationToken = default);

    Task<GenerationResponse?> GetGenerationAsync(
        Guid generationId,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<GenerationResponse>> GetGenerationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        GenerationStatus? status = null,
        CancellationToken cancellationToken = default);

    Task SubmitFeedbackAsync(
        Guid generationId,
        Guid userId,
        SubmitFeedbackRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> CancelGenerationAsync(
        Guid generationId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
