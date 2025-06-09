namespace FitnessAnalyticsHub.Infrastructure.Configuration;

public class StravaConfiguration
{
    public const string SectionName = "Strava";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://www.strava.com/api/v3/";
    public string AuthorizeUrl { get; set; } = "https://www.strava.com/oauth/authorize";
    public string TokenUrl { get; set; } = "https://www.strava.com/oauth/token";
}
