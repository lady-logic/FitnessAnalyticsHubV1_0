﻿namespace AIAssistant.Tests.Infrastructure.Services;

using AIAssistant.Application.DTOs;
using AIAssistant.Infrastructure.Services;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

public class WorkoutAnalysisServiceTests
{
    private WorkoutAnalysisService CreateServiceWithMockConfig(string defaultProvider = "GoogleGemini")
    {
        Mock<IConfiguration> mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["AI:DefaultProvider"]).Returns(defaultProvider);

        Mock<ILogger<WorkoutAnalysisService>> mockLogger = new Mock<ILogger<WorkoutAnalysisService>>();

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
                    Date = DateTime.Now.AddDays(-1),
                },
            },
            AthleteProfile = new AthleteProfileDto
            {
                FitnessLevel = "Intermediate",
                PrimaryGoal = "Endurance",
            },
            AnalysisType = "Performance",
        };
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WhenAIServiceFails_ReturnsFallbackAnalysis()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();

        // Act - AI Services sind null, sollte Fallback triggern
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

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
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AnalysisType = analysisType;

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

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
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = null,
            AnalysisType = "Performance",
        };

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.Contains("0 absolvierte Trainingseinheiten", result.Analysis);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithMultipleWorkouts_CalculatesCorrectStats()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<WorkoutDataDto>
            {
                new WorkoutDataDto { Distance = 5000, Duration = 1800, Calories = 300 },
                new WorkoutDataDto { Distance = 3000, Duration = 1200, Calories = 200 },
            },
            AnalysisType = "Performance",
        };

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

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
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig(provider);
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(provider, result.Provider);
    }

    [Fact]
    public async Task AnalyzeHuggingFaceWorkoutsAsync_ReturnsHuggingFaceProvider()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeHuggingFaceWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("HuggingFace", result.Provider);
    }

    [Fact]
    public async Task AnalyzeGoogleGeminiWorkoutsAsync_ReturnsGoogleGeminiProvider()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeGoogleGeminiWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("GoogleGemini", result.Provider);
    }

    #region Prompt Building Tests
    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithNullAnalysisType_UsesDefaultAnalysis()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AnalysisType = null;

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.True(result.Analysis.Length > 50);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithEmptyAnalysisType_UsesDefaultAnalysis()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AnalysisType = string.Empty;

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.True(result.Analysis.Length > 50);
    }

    #endregion

    #region Athlete Profile Tests

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AthleteProfile = null;

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.True(result.KeyInsights?.Count > 0);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithEmptyAthleteProfile_HandlesGracefully()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AthleteProfile = new AthleteProfileDto
        {
            FitnessLevel = string.Empty,
            PrimaryGoal = string.Empty,
        };

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithNullAthleteProfileProperties_HandlesGracefully()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AthleteProfile = new AthleteProfileDto
        {
            FitnessLevel = null,
            PrimaryGoal = null,
        };

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
    }

    #endregion

    #region Workout Data Edge Cases

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithEmptyWorkoutsList_ReturnsValidFallback()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.RecentWorkouts = new List<WorkoutDataDto>(); // Empty list

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("0", result.Analysis);
        Assert.True(result.KeyInsights?.Count > 0);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithWorkoutsContainingNullValues_HandlesGracefully()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<WorkoutDataDto>
        {
            new WorkoutDataDto
            {
                ActivityType = null,
                Distance = 0,
                Duration = 0,
                Calories = 0,
                Date = DateTime.Now,
            },
        },
            AnalysisType = "Performance",
        };

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.True(result.KeyInsights?.Count > 0);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithLargeNumberOfWorkouts_HandlesCorrectly()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        List<WorkoutDataDto> workouts = new List<WorkoutDataDto>();

        // Create 10 workouts
        for (int i = 0; i < 10; i++)
        {
            workouts.Add(new WorkoutDataDto
            {
                ActivityType = $"Activity{i}",
                Distance = 1000 * (i + 1),
                Duration = 600 * (i + 1),
                Calories = 100 * (i + 1),
                Date = DateTime.Now.AddDays(-i),
            });
        }

        WorkoutAnalysisRequestDto request = new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = workouts,
            AnalysisType = "Performance",
        };

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("10", result.Analysis);
        Assert.True(result.Analysis.Contains("55000") || result.Analysis.Contains("55.0") || result.Analysis.Contains("55,0")); // Total distance
    }

    #endregion

    #region Response Content Validation

    [Fact]
    public async Task AnalyzeWorkoutsAsync_HealthAnalysis_ContainsHealthSpecificInsights()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AnalysisType = "health";

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result.KeyInsights);
        Assert.True(result.KeyInsights.Count >= 3);
        Assert.Contains(result.KeyInsights, insight =>
            insight.Contains("Gesundheit") || insight.Contains("health", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_PerformanceAnalysis_ContainsPerformanceSpecificInsights()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AnalysisType = "performance";

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result.KeyInsights);
        Assert.True(result.KeyInsights.Count >= 3);
        Assert.Contains(result.KeyInsights, insight =>
            insight.Contains("Leistung") || insight.Contains("performance", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_TrendsAnalysis_ContainsTrendsSpecificInsights()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AnalysisType = "trends";

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result.KeyInsights);
        Assert.True(result.KeyInsights.Count >= 3);
        Assert.Contains(result.KeyInsights, insight =>
            insight.Contains("Trend") || insight.Contains("trend", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_AllAnalysisTypes_ContainValidRecommendations()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        string?[] analysisTypes = new[] { "health", "performance", "trends", "general", null };

        foreach (string? analysisType in analysisTypes)
        {
            WorkoutAnalysisRequestDto request = this.CreateTestRequest();
            request.AnalysisType = analysisType;

            // Act
            WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Recommendations);
            Assert.True(result.Recommendations.Count >= 3, $"Failed for analysis type: {analysisType}");
            Assert.All(result.Recommendations, rec =>
            {
                Assert.True(rec.Length > 20, $"Recommendation too short for {analysisType}: {rec}");
                Assert.True(rec.Length < 300, $"Recommendation too long for {analysisType}: {rec}");
            });
        }
    }

    #endregion

    #region Statistics Calculation Tests

    [Fact]
    public async Task AnalyzeWorkoutsAsync_CalculatesCorrectAverageCalories()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<WorkoutDataDto>
        {
            new WorkoutDataDto { Calories = 300 },
            new WorkoutDataDto { Calories = 200 },
            new WorkoutDataDto { Calories = 400 },
        },
            AnalysisType = "Performance",
        };

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        // Average should be 300
        Assert.True(result.Analysis.Contains("300") || result.KeyInsights.Any(i => i.Contains("300")));
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithZeroValues_HandlesGracefully()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<WorkoutDataDto>
        {
            new WorkoutDataDto
            {
                Distance = 0,
                Duration = 0,
                Calories = 0,
                ActivityType = "Rest Day",
            },
        },
            AnalysisType = "Performance",
        };

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.True(result.KeyInsights?.Count > 0);
    }

    #endregion

    #region Edge Case String Handling

    [Theory]
    [InlineData("HEALTH")]
    [InlineData("Health")]
    [InlineData("PERFORMANCE")]
    [InlineData("Performance")]
    [InlineData("TRENDS")]
    [InlineData("Trends")]
    public async Task AnalyzeWorkoutsAsync_WithDifferentCasing_HandlesCorrectly(string analysisType)
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AnalysisType = analysisType;

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
        Assert.True(result.Analysis.Length > 50);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithWhitespaceAnalysisType_HandlesCorrectly()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        request.AnalysisType = "  performance  ";

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
    }

    #endregion

    #region Date and Time Handling

    [Fact]
    public async Task AnalyzeWorkoutsAsync_SetsGeneratedAtToCurrentTime()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = this.CreateTestRequest();
        DateTime beforeTest = DateTime.UtcNow.AddSeconds(-1);

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);
        DateTime afterTest = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.True(result.GeneratedAt >= beforeTest && result.GeneratedAt <= afterTest);
    }

    [Fact]
    public async Task AnalyzeWorkoutsAsync_WithOldWorkoutDates_HandlesCorrectly()
    {
        // Arrange
        WorkoutAnalysisService service = this.CreateServiceWithMockConfig();
        WorkoutAnalysisRequestDto request = new WorkoutAnalysisRequestDto
        {
            RecentWorkouts = new List<WorkoutDataDto>
        {
            new WorkoutDataDto
            {
                Date = DateTime.Now.AddYears(-1), // Very old workout
                ActivityType = "Run",
                Distance = 5000,
                Duration = 1800,
                Calories = 300,
            },
        },
            AnalysisType = "Performance",
        };

        // Act
        WorkoutAnalysisResponseDto result = await service.AnalyzeWorkoutsAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Analysis);
    }

    #endregion
}