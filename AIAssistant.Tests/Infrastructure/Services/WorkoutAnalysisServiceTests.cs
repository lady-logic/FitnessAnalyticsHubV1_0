using AIAssistant.Application.DTOs;
using AIAssistant.Infrastructure.Services;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AIAssistant.Tests.Infrastructure.Services;

public class WorkoutAnalysisServiceTests
{
    private WorkoutAnalysisService CreateServiceWithMockConfig(string defaultProvider = "GoogleGemini")
    {
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:DefaultProvider"]).Returns(defaultProvider);

        var mockLogger = new Mock<ILogger<WorkoutAnalysisService>>();

        // Hauptsächlich Fallback-Logik testen
        return new WorkoutAnalysisService(null, null, mockConfig.Object, mockLogger.Object);
    }

    private WorkoutAnalysisRequestDto CreateTestRequest()
    {
        return new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<WorkoutDataDto>
            {
                new WorkoutDataDto
                {
                    ActivityType = "Run",
                    Distance = 5000,
                    Duration = 1800,
                    Calories = 300,
                    Date = DateTime.Now.AddDays(-1)
                }
            },
            AthleteProfile = new AthleteProfileDto
            {
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Endurance"
            },
            AnalysisType = "Performance"
        };
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WhenAIServiceFails_ReturnsFallbackAnalysis()
    {
        // Arrange
        var service = CreateServiceWithMockConfig();
        var request = CreateTestRequest();

        // Act - AI Services sind null, sollte Fallback triggern
        var result = await service.AnalyzeWorkoutsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.NotNull(result.KeyInsights);
        Assert.NotNull(result.Recommendations);
        Assert.Contains("Leistungsdaten", result.Analysis); // Fallback text
    }

    [Theory]
    [InlineData("health")]
    [InlineData("performance")]
    [InlineData("trends")]
    public async Task AnalyzeWorkoutsAsync_WithDifferentAnalysisTypes_GeneratesValidFallback(string analysisType)
    {
        // Arrange
        var service = CreateServiceWithMockConfig();
        var request = CreateTestRequest();
        request.AnalysisType = analysisType;

        // Act
        var result = await service.AnalyzeWorkoutsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.True(result.Analysis.Length > 50); // Sinnvolle Länge
        Assert.True(result.KeyInsights?.Count > 0);
        Assert.True(result.Recommendations?.Count > 0);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithNoWorkouts_ReturnsValidFallback()
    {
        // Arrange
        var service = CreateServiceWithMockConfig();
        var request = new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = null,
            AnalysisType = "Performance"
        };

        // Act
        var result = await service.AnalyzeWorkoutsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.Contains("0 absolvierte Trainingseinheiten", result.Analysis);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithMultipleWorkouts_CalculatesCorrectStats()
    {
        // Arrange
        var service = CreateServiceWithMockConfig();
        var request = new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<WorkoutDataDto>
            {
                new WorkoutDataDto { Distance = 5000, Duration = 1800, Calories = 300 },
                new WorkoutDataDto { Distance = 3000, Duration = 1200, Calories = 200 }
            },
            AnalysisType = "Performance"
        };

        // Act
        var result = await service.AnalyzeWorkoutsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Analysis.Contains("8000") || result.Analysis.Contains("8.0") || result.Analysis.Contains("8,0"));
        Assert.Contains("2", result.Analysis); // Workout count
        Assert.True(result.Analysis.Length > 100); // Sinnvolle Analyse-Länge
    }

    [Theory]
    [InlineData("HuggingFace")]
    [InlineData("GoogleGemini")]
    public async Task AnalyzeWorkoutsWithProviderAsync_SetsCorrectProvider(string provider)
    {
        // Arrange
        var service = CreateServiceWithMockConfig(provider);
        var request = CreateTestRequest();

        // Act
        var result = await service.AnalyzeWorkoutsAsync(request);

        // Assert
        Assert.Equal(provider, result.Provider);
    }

    [Fact]
    public async Task AnalyzeHuggingFaceWorkoutsAsync_ReturnsHuggingFaceProvider()
    {
        // Arrange
        var service = CreateServiceWithMockConfig();
        var request = CreateTestRequest();

        // Act
        var result = await service.AnalyzeHuggingFaceWorkoutsAsync(request);

        // Assert
        Assert.Equal("HuggingFace", result.Provider);
    }

    [Fact]
    public async Task AnalyzeGoogleGeminiWorkoutsAsync_ReturnsGoogleGeminiProvider()
    {
        // Arrange
        var service = CreateServiceWithMockConfig();
        var request = CreateTestRequest();

        // Act
        var result = await service.AnalyzeGoogleGeminiWorkoutsAsync(request);

        // Assert
        Assert.Equal("GoogleGemini", result.Provider);
    }
}