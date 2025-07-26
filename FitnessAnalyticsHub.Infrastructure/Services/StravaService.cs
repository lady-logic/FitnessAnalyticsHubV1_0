namespace FitnessAnalyticsHub.Infrastructure.Services;

using System.Net;
using System.Text.Json;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Interfaces;
using FitnessAnalyticsHub.Domain.Models;
using FitnessAnalyticsHub.Infrastructure.Configuration;
using FitnessAnalyticsHub.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;

public class StravaService : IStravaService
{
    private readonly HttpClient httpClient;
    private readonly StravaConfiguration config;
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly string redirectUrl;

    public StravaService(IHttpClientFactory httpClientFactory, IOptions<StravaConfiguration> config)
    {
        this.httpClient = httpClientFactory.CreateClient("StravaApi");
        this.config = config.Value;
        this.httpClient.BaseAddress = new Uri(this.config.BaseUrl);
    }

    public Task<string> GetAuthorizationUrlAsync(CancellationToken cancellationToken)
    {
        string authUrl = $"{this.config.AuthorizeUrl}?client_id={this.config.ClientId}" +
                         $"&redirect_uri={Uri.EscapeDataString(this.config.RedirectUrl)}" +
                         $"&response_type=code" +
                         $"&scope=read_all,activity:read_all" +
                         $"&approval_prompt=force";

        return Task.FromResult(authUrl);
    }

    public async Task<TokenInfo> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken)
    {
        FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "client_id", this.config.ClientId },
            { "client_secret", this.config.ClientSecret },
            { "code", code },
            { "grant_type", "authorization_code" },
        });

        HttpResponseMessage response = await this.httpClient.PostAsync(this.config.TokenUrl, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        TokenResponse? tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

        return new TokenInfo
        {
            TokenType = tokenResponse.TokenType,
            AccessToken = tokenResponse.AccessToken,
            ExpiresAt = tokenResponse.ExpiresAt,
            ExpiresIn = tokenResponse.ExpiresIn,
            RefreshToken = tokenResponse.RefreshToken,
        };
    }

    public async Task<Athlete> GetAthleteProfileAsync(string accessToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidStravaTokenException("Access token cannot be null or empty");
        }

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "athlete");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        HttpResponseMessage response = await this.httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        AthleteProfile? athleteProfile = JsonSerializer.Deserialize<AthleteProfile>(content);

        return new Athlete
        {
            StravaId = athleteProfile.Id.ToString(),
            FirstName = athleteProfile.Firstname,
            LastName = athleteProfile.Lastname,
            Username = athleteProfile.Username,
            Email = athleteProfile.Email,
            City = athleteProfile.City,
            Country = athleteProfile.Country,
            ProfilePictureUrl = athleteProfile.Profile,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
    }

    public async Task<IEnumerable<Activity>> GetActivitiesAsync(string accessToken, CancellationToken cancellationToken, int page = 1, int perPage = 30)
    {
        // Token-Validierung
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidStravaTokenException("Access token cannot be null or empty");
        }

        Console.WriteLine($"Requesting activities with token: {accessToken?.Substring(0, 10)}...");

        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"athlete/activities?page={page}&per_page={perPage}");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        Console.WriteLine($"Request URL: {this.httpClient.BaseAddress}{request.RequestUri}");

        HttpResponseMessage response = await this.httpClient.SendAsync(request, cancellationToken);

        Console.WriteLine($"Response Status: {response.StatusCode}");
        Console.WriteLine($"Response Headers:");
        foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
        {
            Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        Console.WriteLine($"Response Content: {content}");

        // Spezifische HTTP-Status-Behandlung
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidStravaTokenException("Access token is invalid or expired");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new StravaApiException("Access forbidden - insufficient permissions", (int)response.StatusCode);
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new StravaApiException("Rate limit exceeded - too many requests", (int)response.StatusCode);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new StravaApiException($"Strava API error: {response.ReasonPhrase}", (int)response.StatusCode);
        }

        List<StravaActivity>? stravaActivities = JsonSerializer.Deserialize<List<StravaActivity>>(content);

        List<Activity> activities = new List<Activity>();
        foreach (StravaActivity stravaActivity in stravaActivities)
        {
            activities.Add(MapToActivity(stravaActivity));
        }

        return activities;
    }

    public async Task<Activity> GetActivityDetailsByIdAsync(string accessToken, string activityId, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"activities/{activityId}");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        HttpResponseMessage response = await this.httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        StravaActivity? stravaActivity = JsonSerializer.Deserialize<StravaActivity>(content);

        return MapToActivity(stravaActivity);
    }

    public async Task<(Athlete athlete, IEnumerable<Activity> activities)> ImportMyActivitiesAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== STRAVA AUTO-IMPORT STARTING ===");

        // Frischen Token generieren
        string accessToken = await this.GenerateFreshTokenAsync(cancellationToken);

        // Athleten-Profil abrufen
        Console.WriteLine("Fetching your athlete profile...");
        Athlete athlete = await this.GetAthleteProfileAsync(accessToken, cancellationToken);
        Console.WriteLine($"✅ Profile loaded: {athlete.FirstName} {athlete.LastName}");

        // Alle Aktivitäten abrufen
        Console.WriteLine("Fetching your activities...");
        IEnumerable<Activity> activities = await this.GetActivitiesAsync(accessToken, cancellationToken);
        Console.WriteLine($"✅ Retrieved {activities.Count()} activities");

        Console.WriteLine("=== STRAVA AUTO-IMPORT COMPLETE ===");

        return (athlete, activities);
    }

    private async Task<string> GenerateFreshTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(this.config.ClientId) || string.IsNullOrEmpty(this.config.ClientSecret))
        {
            throw new StravaConfigurationException("ClientId and ClientSecret must be configured in User Secrets");
        }

        Console.WriteLine("=== STRAVA AUTHORIZATION REQUIRED ===");
        Console.WriteLine("Step 1: Open this URL in your browser:");

        // Authorization URL generieren (mit korrektem Scope!)
        string authUrl = $"{this.config.AuthorizeUrl}?" +
                     $"client_id={this.config.ClientId}&" +
                     $"response_type=code&" +
                     $"redirect_uri=http://localhost&" +
                     $"scope=activity:read_all&" +
                     $"approval_prompt=force";

        Console.WriteLine($"{authUrl}");
        Console.WriteLine();
        Console.WriteLine("Step 2: After authorization, copy the 'code' parameter from the URL");
        Console.WriteLine("Example: http://localhost/?state=&code=COPY_THIS_PART&scope=...");
        Console.WriteLine();
        Console.Write("Paste the authorization code here: ");

        string authCode = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(authCode))
        {
            throw new StravaAuthorizationException("No authorization code provided by user");
        }

        Console.WriteLine("Exchanging code for access token...");

        // Token Exchange über bestehende Methode
        TokenInfo tokenInfo = await this.ExchangeCodeForTokenAsync(authCode, cancellationToken);

        if (string.IsNullOrEmpty(tokenInfo.AccessToken))
        {
            throw new StravaAuthorizationException("Failed to get access token from Strava - received empty token");
        }

        Console.WriteLine("✅ Successfully got access token!");

        return tokenInfo.AccessToken;
    }

    private static Activity MapToActivity(StravaActivity stravaActivity)
    {
        return new Activity
        {
            StravaId = stravaActivity.Id.ToString(),
            Name = stravaActivity.Name,
            Distance = stravaActivity.Distance,
            MovingTime = stravaActivity.MovingTime,
            ElapsedTime = stravaActivity.ElapsedTime,
            TotalElevationGain = stravaActivity.TotalElevationGain,
            SportType = stravaActivity.SportType,
            StartDate = stravaActivity.StartDate,
            StartDateLocal = stravaActivity.StartDateLocal,
            Timezone = stravaActivity.Timezone,
            AverageSpeed = stravaActivity.AverageSpeed,
            MaxSpeed = stravaActivity.MaxSpeed,
            AverageHeartRate = stravaActivity.AverageHeartRate,
            MaxHeartRate = stravaActivity.MaxHeartRate,
            AveragePower = stravaActivity.AveragePower,
            MaxPower = stravaActivity.MaxPower,
            AverageCadence = stravaActivity.AverageCadence,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };
    }

    // Private DTO models to deserialize Strava API responses
    private class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_at")]
        public int ExpiresAt { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
    }

    private class AthleteProfile
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public long Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("username")]
        public string Username { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("firstname")]
        public string Firstname { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("lastname")]
        public string Lastname { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("bio")]
        public string Bio { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("city")]
        public string City { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("country")]
        public string Country { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("profile")]
        public string Profile { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("email")]
        public string Email { get; set; }
    }

    private class StravaActivity
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public long Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("distance")]
        public float Distance { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("moving_time")]
        public int MovingTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("elapsed_time")]
        public int ElapsedTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("total_elevation_gain")]
        public float TotalElevationGain { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("sport_type")]
        public string SportType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("start_date_local")]
        public DateTime StartDateLocal { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("timezone")]
        public string Timezone { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("average_speed")]
        public float? AverageSpeed { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("max_speed")]
        public float? MaxSpeed { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("average_heartrate")]
        public int? AverageHeartRate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("max_heartrate")]
        public int? MaxHeartRate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("average_watts")]
        public double? AveragePower { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("max_watts")]
        public double? MaxPower { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("average_cadence")]
        public double? AverageCadence { get; set; }
    }
}
