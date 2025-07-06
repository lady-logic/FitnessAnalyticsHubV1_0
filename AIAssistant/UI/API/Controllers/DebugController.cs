using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistant.UI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DebugController> _logger;

    public DebugController(IConfiguration configuration, ILogger<DebugController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("config-check")]
    public ActionResult ConfigCheck()
    {
        var apiKey = _configuration["HuggingFace:ApiKey"];

        return Ok(new
        {
            hasApiKey = !string.IsNullOrEmpty(apiKey),
            apiKeyLength = apiKey?.Length ?? 0,
            apiKeyPreview = !string.IsNullOrEmpty(apiKey) ? $"{apiKey.Substring(0, Math.Min(10, apiKey.Length))}..." : "NULL",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            allHuggingFaceKeys = _configuration.GetSection("HuggingFace").GetChildren().Select(x => x.Key).ToArray()
        });
    }

    [HttpGet("test-modern-huggingface")]
    public async Task<ActionResult> TestModernHuggingFace()
    {
        var apiKey = _configuration["HuggingFace:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            return BadRequest("No HuggingFace API key found");
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Teste verschiedene Provider URLs (2025 korrekte URLs)
            var testProviders = new[]
            {
                ("novita", "https://router.huggingface.co/novita/v3/openai/chat/completions", "deepseek-ai/DeepSeek-V3-0324"),
                ("sambanova", "https://router.huggingface.co/sambanova/v1/chat/completions", "Meta-Llama-3.1-8B-Instruct"),
                ("together", "https://router.huggingface.co/together/v1/chat/completions", "meta-llama/Llama-3.2-3B-Instruct-Turbo")
            };

            var results = new List<object>();

            foreach (var (provider, url, model) in testProviders)
            {
                try
                {
                    var testPayload = new
                    {
                        model = model,
                        messages = new[]
                        {
                            new { role = "user", content = "Give me a short fitness motivation!" }
                        },
                        max_tokens = 50,
                        temperature = 0.7,
                        stream = false
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(testPayload);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    _logger.LogInformation("Testing provider: {Provider} with URL: {Url}", provider, url);
                    var response = await httpClient.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    results.Add(new
                    {
                        provider = provider,
                        model = model,
                        url = url,
                        statusCode = (int)response.StatusCode,
                        isSuccess = response.IsSuccessStatusCode,
                        response = responseContent.Length > 500 ? responseContent.Substring(0, 500) + "..." : responseContent
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("SUCCESS with provider: {Provider}!", provider);
                        break; // Stoppe bei erstem erfolgreichen Provider
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        provider = provider,
                        model = model,
                        url = url,
                        error = ex.Message
                    });
                }
            }

            return Ok(new
            {
                message = "Modern HuggingFace Inference Providers Test (Multiple Providers)",
                testResults = results,
                note = "Requires HuggingFace token with 'Inference Providers' scope"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("test-huggingface-service")]
    public async Task<ActionResult> TestHuggingFaceService()
    {
        try
        {
            // Teste direkt über den HuggingFaceService
            var motivationService = HttpContext.RequestServices.GetRequiredService<IMotivationCoachService>();

            var testRequest = new MotivationRequestDto
            {
                AthleteProfile = new Domain.Models.AthleteProfile
                {
                    Name = "TestUser",
                    FitnessLevel = "Intermediate",
                    PrimaryGoal = "General Fitness"
                },
                //RecentWorkouts = new List<Domain.Models.WorkoutData>
                //{
                //    new Domain.Models.WorkoutData
                //    {
                //        Date = DateTime.Now.AddDays(-1),
                //        ActivityType = "Run",
                //        Distance = 5.0,
                //        Duration = 1800,
                //        Calories = 300
                //    }
                //},
                //PreferredTone = "Encouraging",
                //ContextualInfo = "Testing HuggingFace integration"
            };

            _logger.LogInformation("Testing HuggingFaceService through MotivationCoachService...");
            var result = await motivationService.GetHuggingFaceMotivationalMessageAsync(testRequest);

            return Ok(new
            {
                message = "HuggingFaceService Test via MotivationCoachService",
                isSuccess = !string.IsNullOrEmpty(result.MotivationalMessage),
                hasMotivation = !string.IsNullOrEmpty(result.MotivationalMessage),
                hasQuote = !string.IsNullOrEmpty(result.Quote),
                hasTips = result.ActionableTips?.Any() == true,
                motivationLength = result.MotivationalMessage?.Length ?? 0,
                containsAINote = result.MotivationalMessage?.Contains("AI analysis temporarily unavailable") == true,
                generatedAt = result.GeneratedAt,
                fullResponse = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HuggingFaceService test failed");
            return StatusCode(500, new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray()
            });
        }
    }

    [HttpGet("direct-huggingface-test")]
    public async Task<ActionResult> DirectHuggingFaceTest()
    {
        var apiKey = _configuration["HuggingFace:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            return BadRequest("No HuggingFace API key found in configuration");
        }

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Teste direkt HuggingFace API mit korrektem Model-Namen
            var testUrl = "https://api-inference.huggingface.co/models/openai-community/gpt2";
            var testPayload = new
            {
                inputs = "Great workout today! I feel",
                parameters = new
                {
                    max_length = 50,
                    temperature = 0.7,
                    do_sample = true,
                    return_full_text = false
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(testPayload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("Testing HuggingFace API directly...");
            var response = await httpClient.PostAsync(testUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            return Ok(new
            {
                statusCode = (int)response.StatusCode,
                isSuccess = response.IsSuccessStatusCode,
                responseBody = responseContent,
                requestUrl = testUrl,
                headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value))
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Direct HuggingFace test failed");
            return StatusCode(500, new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet("test-no-auth")]
    public async Task<ActionResult> TestWithoutAuth()
    {
        try
        {
            using var httpClient = new HttpClient();
            // KEIN Authorization Header!

            var testUrl = "https://api-inference.huggingface.co/models/openai-community/gpt2";
            var testPayload = new
            {
                inputs = "Hello, I am a fitness coach and"
            };

            var json = System.Text.Json.JsonSerializer.Serialize(testPayload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _logger.LogInformation("Testing WITHOUT authentication...");
            var response = await httpClient.PostAsync(testUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return Ok(new
            {
                message = "Test without authentication",
                statusCode = (int)response.StatusCode,
                isSuccess = response.IsSuccessStatusCode,
                response = responseContent,
                headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value))
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("health")]
    public ActionResult HealthCheck()
    {
        try
        {
            var config = new
            {
                hasHuggingFaceKey = !string.IsNullOrEmpty(_configuration["HuggingFace:ApiKey"]),
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                timestamp = DateTime.UtcNow
            };

            return Ok(new
            {
                status = "healthy",
                message = "Debug controller is responding",
                configuration = config,
                availableTests = new[]
                {
                    "GET /api/Debug/config-check",
                    "GET /api/Debug/test-modern-huggingface",
                    "GET /api/Debug/test-huggingface-service",
                    "GET /api/Debug/direct-huggingface-test",
                    "GET /api/Debug/test-no-auth"
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
}