namespace AIAssistant._04_UI.API.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class HuggingFaceTestController : ControllerBase
{
    private readonly IConfiguration configuration;
    private readonly ILogger<HuggingFaceTestController> logger;

    public HuggingFaceTestController(IConfiguration configuration, ILogger<HuggingFaceTestController> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    [HttpGet("test-token")]
    public async Task<ActionResult> TestToken()
    {
        var apiKey = this.configuration["HuggingFace:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            return this.BadRequest("No HuggingFace API key found");
        }

        try
        {
            using var httpClient = new HttpClient();

            // Test 1: Teste User Info (Token validation)
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            this.logger.LogInformation("Testing HuggingFace token with whoami endpoint...");
            var whoamiResponse = await httpClient.GetAsync("https://huggingface.co/api/whoami");
            var whoamiContent = await whoamiResponse.Content.ReadAsStringAsync();

            this.logger.LogInformation("Whoami response: {Response}", whoamiContent);

            // Test 2: Liste verfügbare Models
            this.logger.LogInformation("Testing model list endpoint...");
            var modelsResponse = await httpClient.GetAsync("https://huggingface.co/api/models?search=gpt2&limit=5");
            var modelsContent = await modelsResponse.Content.ReadAsStringAsync();

            return this.Ok(new
            {
                tokenValidation = new
                {
                    statusCode = (int)whoamiResponse.StatusCode,
                    isSuccess = whoamiResponse.IsSuccessStatusCode,
                    response = whoamiContent,
                },
                availableModels = new
                {
                    statusCode = (int)modelsResponse.StatusCode,
                    isSuccess = modelsResponse.IsSuccessStatusCode,
                    response = modelsContent.Length > 1000 ? modelsContent.Substring(0, 1000) + "..." : modelsContent,
                },
                apiKeyPreview = $"{apiKey.Substring(0, 10)}...",
            });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Token test failed");
            return this.StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("test-simple-model")]
    public async Task<ActionResult> TestSimpleModel()
    {
        var apiKey = this.configuration["HuggingFace:ApiKey"];

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Test mit dem einfachsten verfügbaren Model
            var testUrl = "https://api-inference.huggingface.co/models/distilbert-base-uncased";
            var testPayload = new
            {
                inputs = "Hello world",
            };

            var json = System.Text.Json.JsonSerializer.Serialize(testPayload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            this.logger.LogInformation("Testing with DistilBERT model...");
            var response = await httpClient.PostAsync(testUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return this.Ok(new
            {
                model = "distilbert-base-uncased",
                statusCode = (int)response.StatusCode,
                isSuccess = response.IsSuccessStatusCode,
                response = responseContent,
                url = testUrl,
            });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("test-text-generation")]
    public async Task<ActionResult> TestTextGeneration()
    {
        var apiKey = this.configuration["HuggingFace:ApiKey"];

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Teste verschiedene Text-Generation Models
            var modelsToTest = new[]
            {
                "gpt2",
                "distilgpt2",
                "microsoft/DialoGPT-medium",
                "EleutherAI/gpt-neo-125M",
            };

            var results = new List<object>();

            foreach (var model in modelsToTest)
            {
                try
                {
                    var url = $"https://api-inference.huggingface.co/models/{model}";
                    var payload = new { inputs = "Hello, I am" };
                    var json = System.Text.Json.JsonSerializer.Serialize(payload);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(url, content);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    results.Add(new
                    {
                        model = model,
                        statusCode = (int)response.StatusCode,
                        isSuccess = response.IsSuccessStatusCode,
                        response = responseContent.Length > 200 ? responseContent.Substring(0, 200) + "..." : responseContent,
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        this.logger.LogInformation("SUCCESS with model: {Model}", model);
                        break; // Stoppe bei erstem funktionierenden Model
                    }
                }
                catch (Exception ex)
                {
                    results.Add(new
                    {
                        model = model,
                        error = ex.Message,
                    });
                }
            }

            return this.Ok(new { testResults = results });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message });
        }
    }
}