using FitnessAnalyticsHub.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FitnessAnalyticsHub.Infrastructure.Services.AIAssistant;

public class AIAssistantClient : IAIAssistantClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public AIAssistantClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["AIAssistant:BaseUrl"];
    }

    //public async Task<WorkoutAnalysisResult> AnalyzeWorkoutAsync(WorkoutData data)
    //{
    //    var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/workoutanalysis", data);
    //    response.EnsureSuccessStatusCode();
    //    return await response.Content.ReadFromJsonAsync<WorkoutAnalysisResult>();
    //}

    //public async Task<string> GetMotivationalMessageAsync(UserProfile profile)
    //{
    //    var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/motivation", profile);
    //    response.EnsureSuccessStatusCode();
    //    return await response.Content.ReadAsStringAsync();
    //}
}
