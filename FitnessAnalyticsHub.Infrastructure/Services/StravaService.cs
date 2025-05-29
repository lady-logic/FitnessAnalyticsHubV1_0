using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Interfaces;
using FitnessAnalyticsHub.Domain.Models;
using FitnessAnalyticsHub.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace FitnessAnalyticsHub.Infrastructure.Services
{
    public class StravaService : IStravaService
    {
        private readonly HttpClient _httpClient;
        private readonly StravaConfiguration _config;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUrl;

        public StravaService(IHttpClientFactory httpClientFactory, IOptions<StravaConfiguration> config)
        {
            _httpClient = httpClientFactory.CreateClient("StravaApi");
            _config = config.Value;
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }

        public Task<string> GetAuthorizationUrlAsync()
        {
            string authUrl = $"{_config.AuthorizeUrl}?client_id={_config.ClientId}" +
                             $"&redirect_uri={Uri.EscapeDataString(_config.RedirectUrl)}" +
                             $"&response_type=code" +
                             $"&scope=read,activity:read_all,profile:read_all" +  
                             $"&approval_prompt=force";  // Erzwingt neue Berechtigung

            return Task.FromResult(authUrl);
        }

        public async Task<TokenInfo> ExchangeCodeForTokenAsync(string code)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "client_id", _config.ClientId },
                { "client_secret", _config.ClientSecret },
                { "code", code },
                { "grant_type", "authorization_code" }
            });

            var response = await _httpClient.PostAsync(_config.TokenUrl, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

            return new TokenInfo
            {
                TokenType = tokenResponse.TokenType,
                AccessToken = tokenResponse.AccessToken,
                ExpiresAt = tokenResponse.ExpiresAt,
                ExpiresIn = tokenResponse.ExpiresIn,
                RefreshToken = tokenResponse.RefreshToken
            };
        }

        public async Task<Athlete> GetAthleteProfileAsync(string accessToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "athlete");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var athleteProfile = JsonSerializer.Deserialize<AthleteProfile>(content);

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
                UpdatedAt = DateTime.Now
            };
        }

        public async Task<IEnumerable<Activity>> GetActivitiesAsync(string accessToken, int page = 1, int perPage = 30)
        {
            Console.WriteLine($"Requesting activities with token: {accessToken?.Substring(0, 10)}...");

            using var request = new HttpRequestMessage(HttpMethod.Get, $"athlete/activities?page={page}&per_page={perPage}");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            Console.WriteLine($"Request URL: {_httpClient.BaseAddress}{request.RequestUri}");

            var response = await _httpClient.SendAsync(request);

            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Headers:");
            foreach (var header in response.Headers)
            {
                Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {content}");

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to get activities. Status: {response.StatusCode}, Content: {content}");
            }

            var stravaActivities = JsonSerializer.Deserialize<List<StravaActivity>>(content);

            var activities = new List<Activity>();
            foreach (var stravaActivity in stravaActivities)
            {
                activities.Add(MapToActivity(stravaActivity));
            }

            return activities;
        }

        public async Task<Activity> GetActivityDetailsByIdAsync(string accessToken, string activityId)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"activities/{activityId}");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var stravaActivity = JsonSerializer.Deserialize<StravaActivity>(content);

            return MapToActivity(stravaActivity);
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
                UpdatedAt = DateTime.Now
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
}
