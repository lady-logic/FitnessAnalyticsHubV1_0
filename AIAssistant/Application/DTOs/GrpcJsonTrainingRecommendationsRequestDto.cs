using System.ComponentModel.DataAnnotations;

namespace FitnessAnalyticsHub.AIAssistant.Application.DTOs;

/// <summary>
/// Request DTO für Training Recommendations
/// </summary>
public class GrpcJsonTrainingRecommendationsRequestDto
{
    [Required]
    public int AthleteId { get; set; }
}
