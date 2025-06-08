namespace FitnessAnalyticsHub.Application.DTOs;

public class AthleteDto
{
    public int Id { get; set; }
    public string? StravaId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
