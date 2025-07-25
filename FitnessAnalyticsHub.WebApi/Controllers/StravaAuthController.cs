﻿namespace FitnessAnalyticsHub.WebApi.Controllers;

using FitnessAnalyticsHub.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

[ApiController]
[Route("api/[controller]")]
public class StravaAuthController : ControllerBase
{
    private readonly IStravaService stravaService;

    public StravaAuthController(IStravaService stravaService)
    {
        this.stravaService = stravaService;
    }

    /// <summary>
    /// Schritt 1: Leitet den User zu Strava zur Autorisierung weiter
    /// </summary>
    [HttpGet("login")]
    public async Task<IActionResult> Login(CancellationToken cancellationToken)
    {
        try
        {
            string authUrl = await this.stravaService.GetAuthorizationUrlAsync(cancellationToken);
            Console.WriteLine($"Redirecting to: {authUrl}");

            // Redirect zu Strava
            return this.Redirect(authUrl);
        }
        catch (Exception ex)
        {
            return this.BadRequest($"Error generating auth URL: {ex.Message}");
        }
    }

    /// <summary>
    /// Schritt 2: Callback von Strava - tauscht Code gegen Token
    /// </summary>
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string scope, [FromQuery] string error, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(error))
            {
                return this.BadRequest($"Authorization failed: {error}");
            }

            if (string.IsNullOrEmpty(code))
            {
                return this.BadRequest("No authorization code received");
            }

            Console.WriteLine($"Received code: {code}");
            Console.WriteLine($"Received scope: {scope}");

            // Tausche Code gegen Token
            Domain.Models.TokenInfo tokenInfo = await this.stravaService.ExchangeCodeForTokenAsync(code, cancellationToken);

            Console.WriteLine($"Token received: {tokenInfo.AccessToken?.Substring(0, 10)}...");
            Console.WriteLine($"Token expires at: {DateTimeOffset.FromUnixTimeSeconds(tokenInfo.ExpiresAt)}");

            // WICHTIG: Prüfe hier, welche Scopes das Token wirklich hat
            Console.WriteLine($"Token scopes: {scope}");

            // Teste sofort das Token mit Athletendaten
            Domain.Entities.Athlete athlete = await this.stravaService.GetAthleteProfileAsync(tokenInfo.AccessToken, cancellationToken);
            Console.WriteLine($"Athlete: {athlete.FirstName} {athlete.LastName}");

            // BEVOR du Activities abrufst, prüfe den Scope:
            if (!scope.Contains("read_all"))
            {
                return this.BadRequest($"Missing required scope. Got: {scope}, but need 'read_all' for activities");
            }

            // Jetzt erst Activities testen:
            IEnumerable<Domain.Entities.Activity> activities = await this.stravaService.GetActivitiesAsync(tokenInfo.AccessToken, cancellationToken, 1, 5);
            Console.WriteLine($"Found {activities.Count()} activities");

            return this.Ok(new
            {
                message = "Authorization successful!",
                athlete = new { athlete.FirstName, athlete.LastName, athlete.Username },
                token = new
                {
                    accessToken = tokenInfo.AccessToken,
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenInfo.ExpiresAt),
                    refreshToken = tokenInfo.RefreshToken,
                    receivedScopes = scope,  // Zeige die wirklich erhaltenen Scopes
                },
                activitiesCount = activities.Count(),
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Callback error: {ex.Message}");
            return this.BadRequest($"Error during authorization: {ex.Message}");
        }
    }

    /// <summary>
    /// Schritt 3: Test-Endpoint um Aktivitäten mit gespeichertem Token abzurufen
    /// </summary>
    [HttpGet("test-activities")]
    public async Task<IActionResult> TestActivities([FromQuery] string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return this.BadRequest("Access token required");
            }

            IEnumerable<Domain.Entities.Activity> activities = await this.stravaService.GetActivitiesAsync(accessToken, cancellationToken, 1, 10);

            return this.Ok(new
            {
                message = "Activities retrieved successfully!",
                count = activities.Count(),
                activities = activities.Select(a => new
                {
                    a.Name,
                    a.SportType,
                    a.Distance,
                    a.StartDate,
                    MovingTimeMinutes = a.MovingTime / 60,
                }),
            });
        }
        catch (Exception ex)
        {
            return this.BadRequest($"Error getting activities: {ex.Message}");
        }
    }
}
