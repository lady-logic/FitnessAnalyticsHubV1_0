namespace FitnessAnalyticsHub.AIAssistant.UI.API.Services;

using Fitnessanalyticshub;
using global::AIAssistant.Application.Interfaces;
using global::AIAssistant.Extensions;
using Grpc.Core;

public class MotivationGrpcService : MotivationService.MotivationServiceBase
{
    private readonly IMotivationCoachService motivationCoachService;
    private readonly ILogger<MotivationGrpcService> logger;

    public MotivationGrpcService(
        IMotivationCoachService motivationCoachService,
        ILogger<MotivationGrpcService> logger)
    {
        this.motivationCoachService = motivationCoachService;
        this.logger = logger;
    }

    public override async Task<global::Fitnessanalyticshub.MotivationResponse> GetMotivation(
        global::Fitnessanalyticshub.MotivationRequest request,
        ServerCallContext context)
    {
        try
        {
            this.logger.LogInformation(
                "gRPC: Received motivation request for athlete: {Name}",
                request.AthleteProfile?.Name ?? "Unknown");

            // Konvertiere gRPC Request zu Application DTO
            var motivationRequest = request.ToMotivationRequestDto();

            // Rufe den HuggingFace Service auf!
            var response = await this.motivationCoachService.GetHuggingFaceMotivationalMessageAsync(motivationRequest);

            // Konvertiere zurück zu gRPC Response
            var grpcResponse = new MotivationResponse
            {
                MotivationalMessage = response.MotivationalMessage ?? string.Empty,
                Quote = response.Quote ?? string.Empty,
                GeneratedAt = response.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            };

            // ActionableTips hinzufügen
            if (response.ActionableTips != null)
            {
                grpcResponse.ActionableTips.AddRange(response.ActionableTips);
            }

            this.logger.LogInformation("gRPC: Successfully generated motivation response");
            return grpcResponse;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "gRPC: Error generating motivation");

            // gRPC Exception werfen
            throw new RpcException(new Status(
                StatusCode.Internal,
                $"Failed to generate motivation: {ex.Message}"));
        }
    }
}
