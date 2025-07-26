namespace AIAssistant.UI.API.Controllers;

using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly ILogger<DebugController> logger;

    public DebugController(IConfiguration configuration, ILogger<DebugController> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    [HttpGet("config-check")]
    public Task<ActionResult> ConfigCheck()
    {
        string? apiKey = this.configuration["HuggingFace:ApiKey"];
        return Task.FromResult<ActionResult>(this.Ok(new
        {
            hasApiKey = !string.IsNullOrEmpty(apiKey),
            apiKeyLength = apiKey?.Length ?? 0,
            apiKeyPreview = !string.IsNullOrEmpty(apiKey) ? $"{apiKey.Substring(0, Math.Min(10, apiKey.Length))}..." : "NULL",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            allHuggingFaceKeys = this.configuration.GetSection("HuggingFace").GetChildren().Select(x => x.Key).ToArray(),
        }));
    }

    [HttpGet("test-modern-huggingface")]
    public async Task<ActionResult> TestModernHuggingFace(CancellationToken cancellationToken)
    {
        string? apiKey = this.configuration["HuggingFace:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            return this.BadRequest("No HuggingFace API key found");
        }

        try
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Teste verschiedene Provider URLs (2025 korrekte URLs)
            (string, string, string)[] testProviders = new[]
            {
                ("novita", "https://router.huggingface.co/novita/v3/openai/chat/completions", "deepseek-ai/DeepSeek-V3-0324"),
                ("sambanova", "https://router.huggingface.co/sambanova/v1/chat/completions", "Meta-Llama-3.1-8B-Instruct"),
                ("together", "https://router.huggingface.co/together/v1/chat/completions", "meta-llama/Llama-3.2-3B-Instruct-Turbo"),
            };

            List<object> results = new List<object>();

            foreach ((string provider, string url, string model) in testProviders)
            {
                try
                {
                    var testPayload = new
                    {
                        model = model,
                        messages = new[]
                        {
                            new { role = "user", content = "Give me a short fitness motivation!" },
                        },
                        max_tokens = 50,
                        temperature = 0.7,
                        stream = false,
                    };

                    string json = System.Text.Json.JsonSerializer.Serialize(testPayload);
                    StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    this.logger.LogInformation("Testing provider: {Provider} with URL: {Url}", provider, url);
                    HttpResponseMessage response = await httpClient.PostAsync(url, content, cancellationToken);
                    string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                    results.Add(new
                    {
                        provider = provider,
                        model = model,
                        url = url,
                        statusCode = (int)response.StatusCode,
                        isSuccess = response.IsSuccessStatusCode,
                        response = responseContent.Length > 500 ? responseContent.Substring(0, 500) + "..." : responseContent,
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        this.logger.LogInformation("SUCCESS with provider: {Provider}!", provider);
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
                        error = ex.Message,
                    });
                }
            }

            return this.Ok(new
            {
                message = "Modern HuggingFace Inference Providers Test (Multiple Providers)",
                testResults = results,
                note = "Requires HuggingFace token with 'Inference Providers' scope",
            });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("test-huggingface-service")]
    public async Task<ActionResult> TestHuggingFaceService(CancellationToken cancellationToken)
    {
        try
        {
            // Teste direkt über den HuggingFaceService
            IMotivationCoachService motivationService = this.HttpContext.RequestServices.GetRequiredService<IMotivationCoachService>();

            MotivationRequestDto testRequest = new MotivationRequestDto
            {
                AthleteProfile = new AthleteProfileDto
                {
                    Name = "TestUser",
                    FitnessLevel = "Intermediate",
                    PrimaryGoal = "General Fitness",
                },

                // RecentWorkouts = new List<Domain.Models.WorkoutData>
                // {
                //    new Domain.Models.WorkoutData
                //    {
                //        Date = DateTime.Now.AddDays(-1),
                //        ActivityType = "Run",
                //        Distance = 5.0,
                //        Duration = 1800,
                //        Calories = 300
                //    }
                // },
                // PreferredTone = "Encouraging",
                // ContextualInfo = "Testing HuggingFace integration"
            };

            this.logger.LogInformation("Testing HuggingFaceService through MotivationCoachService...");
            Application.DTOs.MotivationResponseDto result = await motivationService.GetHuggingFaceMotivationalMessageAsync(testRequest, cancellationToken);

            return this.Ok(new
            {
                message = "HuggingFaceService Test via MotivationCoachService",
                isSuccess = !string.IsNullOrEmpty(result.MotivationalMessage),
                hasMotivation = !string.IsNullOrEmpty(result.MotivationalMessage),
                hasQuote = !string.IsNullOrEmpty(result.Quote),
                hasTips = result.ActionableTips?.Any() == true,
                motivationLength = result.MotivationalMessage?.Length ?? 0,
                containsAINote = result.MotivationalMessage?.Contains("AI analysis temporarily unavailable") == true,
                generatedAt = result.GeneratedAt,
                fullResponse = result,
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "HuggingFaceService test failed");
            return this.StatusCode(500, new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray(),
            });
        }
    }

    [HttpGet("direct-huggingface-test")]
    public async Task<ActionResult> DirectHuggingFaceTest(CancellationToken cancellationToken)
    {
        string? apiKey = this.configuration["HuggingFace:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            return this.BadRequest("No HuggingFace API key found in configuration");
        }

        try
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Teste direkt HuggingFace API mit korrektem Model-Namen
            string testUrl = "https://api-inference.huggingface.co/models/openai-community/gpt2";
            var testPayload = new
            {
                inputs = "Great workout today! I feel",
                parameters = new
                {
                    max_length = 50,
                    temperature = 0.7,
                    do_sample = true,
                    return_full_text = false,
                },
            };

            string json = System.Text.Json.JsonSerializer.Serialize(testPayload);
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            this.logger.LogInformation("Testing HuggingFace API directly...");
            HttpResponseMessage response = await httpClient.PostAsync(testUrl, content, cancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            return this.Ok(new
            {
                statusCode = (int)response.StatusCode,
                isSuccess = response.IsSuccessStatusCode,
                responseBody = responseContent,
                requestUrl = testUrl,
                headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value)),
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Direct HuggingFace test failed");
            return this.StatusCode(500, new
            {
                error = ex.Message,
                stackTrace = ex.StackTrace,
            });
        }
    }

    [HttpGet("test-no-auth")]
    public async Task<ActionResult> TestWithoutAuth(CancellationToken cancellationToken)
    {
        try
        {
            using HttpClient httpClient = new HttpClient();

            // KEIN Authorization Header!
            string testUrl = "https://api-inference.huggingface.co/models/openai-community/gpt2";
            var testPayload = new
            {
                inputs = "Hello, I am a fitness coach and",
            };

            string json = System.Text.Json.JsonSerializer.Serialize(testPayload);
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            this.logger.LogInformation("Testing WITHOUT authentication...");
            HttpResponseMessage response = await httpClient.PostAsync(testUrl, content, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            return this.Ok(new
            {
                message = "Test without authentication",
                statusCode = (int)response.StatusCode,
                isSuccess = response.IsSuccessStatusCode,
                response = responseContent,
                headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value)),
            });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("health")]
    public Task<ActionResult> HealthCheck()
    {
        var config = new
        {
            hasHuggingFaceKey = !string.IsNullOrEmpty(this.configuration["HuggingFace:ApiKey"]),
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            timestamp = DateTime.UtcNow,
        };

        return Task.FromResult<ActionResult>(this.Ok(new
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
            "GET /api/Debug/test-no-auth",
            },
        }));
    }
}