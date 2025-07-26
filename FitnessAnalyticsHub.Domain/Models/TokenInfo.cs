namespace FitnessAnalyticsHub.Domain.Models;

public class TokenInfo
{
    public string TokenType { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public int ExpiresAt { get; set; }

    public int ExpiresIn { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
}
