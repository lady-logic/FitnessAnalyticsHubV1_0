using AIAssistant.Application.Interfaces;
using AIAssistant.Extensions;
using Fitnessanalyticshub;
using Grpc.Core;

namespace FitnessAnalyticsHub.AIAssistant.UI.API.Services;

public class MotivationGrpcService : MotivationService.MotivationServiceBase
{
    private readonly IMotivationCoachService _motivationCoachService;
    private readonly ILogger<MotivationGrpcService> _logger;

    public MotivationGrpcService(
        IMotivationCoachService motivationCoachService,
        ILogger<MotivationGrpcService> logger)
    {
        _motivationCoachService = motivationCoachService;
        _logger = logger;
    }

    public override async Task<global::Fitnessanalyticshub.MotivationResponse> GetMotivation(
        global::Fitnessanalyticshub.MotivationRequest request,
        ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("gRPC: Received motivation request for athlete: {Name}",
                request.AthleteProfile?.Name ?? "Unknown");

            // Konvertiere gRPC Request zu Application DTO
            var motivationRequest = request.ToMotivationRequestDto();

            // Rufe den HuggingFace Service auf!
            var response = await _motivationCoachService.GetHuggingFaceMotivationalMessageAsync(motivationRequest);

            // Konvertiere zurück zu gRPC Response
            var grpcResponse = new MotivationResponse
            {
                MotivationalMessage = response.MotivationalMessage ?? "",
                Quote = response.Quote ?? "",
                GeneratedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            };

            // ActionableTips hinzufügen
            if (response.ActionableTips != null)
            {
                grpcResponse.ActionableTips.AddRange(response.ActionableTips);
            }

            _logger.LogInformation("gRPC: Successfully generated motivation response");
            return grpcResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "gRPC: Error generating motivation");

            // gRPC Exception werfen
            throw new RpcException(new Status(StatusCode.Internal,
                $"Failed to generate motivation: {ex.Message}"));
        }
    }
}
