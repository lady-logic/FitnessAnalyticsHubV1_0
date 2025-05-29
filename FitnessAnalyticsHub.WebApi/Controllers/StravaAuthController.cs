using FitnessAnalyticsHub.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FitnessAnalyticsHub.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class StravaAuthController : ControllerBase
    {
        private readonly IStravaService _stravaService;

        public StravaAuthController(IStravaService stravaService)
        {
            _stravaService = stravaService;
        }

        /// <summary>
        /// Schritt 1: Leitet den User zu Strava zur Autorisierung weiter
        /// </summary>
        [HttpGet("login")]
        public async Task<IActionResult> Login()
        {
            try
            {
                var authUrl = await _stravaService.GetAuthorizationUrlAsync();
                Console.WriteLine($"Redirecting to: {authUrl}");

                // Redirect zu Strava
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating auth URL: {ex.Message}");
            }
        }

        /// <summary>
        /// Schritt 2: Callback von Strava - tauscht Code gegen Token
        /// </summary>
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string scope, [FromQuery] string error)
        {
            try
            {
                // Prüfe auf Fehler
                if (!string.IsNullOrEmpty(error))
                {
                    return BadRequest($"Authorization failed: {error}");
                }

                if (string.IsNullOrEmpty(code))
                {
                    return BadRequest("No authorization code received");
                }

                Console.WriteLine($"Received code: {code}");
                Console.WriteLine($"Received scope: {scope}");

                // Tausche Code gegen Token
                var tokenInfo = await _stravaService.ExchangeCodeForTokenAsync(code);

                Console.WriteLine($"Token received: {tokenInfo.AccessToken?.Substring(0, 10)}...");
                Console.WriteLine($"Token expires at: {DateTimeOffset.FromUnixTimeSeconds(tokenInfo.ExpiresAt)}");
                Console.WriteLine($"Scopes: {scope}");

                // Teste sofort das Token mit Athletendaten
                var athlete = await _stravaService.GetAthleteProfileAsync(tokenInfo.AccessToken);
                Console.WriteLine($"Athlete: {athlete.FirstName} {athlete.LastName}");

                // Teste Aktivitäten
                var activities = await _stravaService.GetActivitiesAsync(tokenInfo.AccessToken, 1, 5);
                Console.WriteLine($"Found {activities.Count()} activities");

                return Ok(new
                {
                    message = "Authorization successful!",
                    athlete = new { athlete.FirstName, athlete.LastName, athlete.Username },
                    token = new
                    {
                        accessToken = tokenInfo.AccessToken,
                        expiresAt = DateTimeOffset.FromUnixTimeSeconds(tokenInfo.ExpiresAt),
                        refreshToken = tokenInfo.RefreshToken,
                        scopes = scope
                    },
                    activitiesCount = activities.Count()
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Callback error: {ex.Message}");
                return BadRequest($"Error during authorization: {ex.Message}");
            }
        }

        /// <summary>
        /// Schritt 3: Test-Endpoint um Aktivitäten mit gespeichertem Token abzurufen
        /// </summary>
        [HttpGet("test-activities")]
        public async Task<IActionResult> TestActivities([FromQuery] string accessToken)
        {
            try
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    return BadRequest("Access token required");
                }

                var activities = await _stravaService.GetActivitiesAsync(accessToken, 1, 10);

                return Ok(new
                {
                    message = "Activities retrieved successfully!",
                    count = activities.Count(),
                    activities = activities.Select(a => new
                    {
                        a.Name,
                        a.SportType,
                        a.Distance,
                        a.StartDate,
                        MovingTimeMinutes = a.MovingTime / 60
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting activities: {ex.Message}");
            }
        }
    }
}
