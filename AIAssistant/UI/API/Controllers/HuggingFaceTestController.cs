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
    public async Task<ActionResult> TestToken(CancellationToken cancellationToken)
    {
        string? apiKey = this.configuration["HuggingFace:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            return this.BadRequest("No HuggingFace API key found");
        }

        try
        {
            using HttpClient httpClient = new HttpClient();

            // Test 1: Teste User Info (Token validation)
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            this.logger.LogInformation("Testing HuggingFace token with whoami endpoint...");
            HttpResponseMessage whoamiResponse = await httpClient.GetAsync("https://huggingface.co/api/whoami", cancellationToken);
            string whoamiContent = await whoamiResponse.Content.ReadAsStringAsync(cancellationToken);

            this.logger.LogInformation("Whoami response: {Response}", whoamiContent);

            // Test 2: Liste verfügbare Models
            this.logger.LogInformation("Testing model list endpoint...");
            HttpResponseMessage modelsResponse = await httpClient.GetAsync("https://huggingface.co/api/models?search=gpt2&limit=5", cancellationToken);
            string modelsContent = await modelsResponse.Content.ReadAsStringAsync(cancellationToken);

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
    public async Task<ActionResult> TestSimpleModel(CancellationToken cancellationToken)
    {
        string? apiKey = this.configuration["HuggingFace:ApiKey"];

        try
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Test mit dem einfachsten verfügbaren Model
            string testUrl = "https://api-inference.huggingface.co/models/distilbert-base-uncased";
            var testPayload = new
            {
                inputs = "Hello world",
            };

            string json = System.Text.Json.JsonSerializer.Serialize(testPayload);
            StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            this.logger.LogInformation("Testing with DistilBERT model...");
            HttpResponseMessage response = await httpClient.PostAsync(testUrl, content, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

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
    public async Task<ActionResult> TestTextGeneration(CancellationToken cancellationToken)
    {
        string? apiKey = this.configuration["HuggingFace:ApiKey"];

        try
        {
            using HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            // Teste verschiedene Text-Generation Models
            string[] modelsToTest = new[]
            {
                "gpt2",
                "distilgpt2",
                "microsoft/DialoGPT-medium",
                "EleutherAI/gpt-neo-125M",
            };

            List<object> results = new List<object>();

            foreach (string? model in modelsToTest)
            {
                try
                {
                    string url = $"https://api-inference.huggingface.co/models/{model}";
                    var payload = new { inputs = "Hello, I am" };
                    string json = System.Text.Json.JsonSerializer.Serialize(payload);
                    StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await httpClient.PostAsync(url, content, cancellationToken);
                    string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

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