using AIAssistant.Application.DTOs;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Grpc.Core;
using Moq;

namespace AIAssistant.Tests.Helpers;

public static class GrpcTestExtensions
{
    #region gRPC Request Builders

    /// <summary>
    /// Erstellt eine gRPC MotivationRequest für Tests
    /// </summary>
    public static global::Fitnessanalyticshub.MotivationRequest CreateGrpcMotivationRequest(
        string athleteName = "Test Athlete",
        string fitnessLevel = "Intermediate",
        string primaryGoal = "General Fitness")
    {
        return new global::Fitnessanalyticshub.MotivationRequest
        {
            AthleteProfile = new global::Fitnessanalyticshub.AthleteProfile
            {
                Name = athleteName,
                FitnessLevel = fitnessLevel,
                PrimaryGoal = primaryGoal
            }
        };
    }

    /// <summary>
    /// Erstellt eine gRPC WorkoutAnalysisRequest für Tests
    /// </summary>
    public static global::Fitnessanalyticshub.WorkoutAnalysisRequest CreateGrpcWorkoutAnalysisRequest(
        string? preferredAiProvider = null,
        string analysisType = "Performance",
        int workoutCount = 2)
    {
        var request = new global::Fitnessanalyticshub.WorkoutAnalysisRequest
        {
            PreferredAiProvider = preferredAiProvider,
            AnalysisType = analysisType
        };

        // Füge Demo-Workouts hinzu
        for (int i = 0; i < workoutCount; i++)
        {
            request.RecentWorkouts.Add(new global::Fitnessanalyticshub.Workout
            {
                Date = DateTime.Now.AddDays(-(i + 1)).ToString("yyyy-MM-dd"),
                ActivityType = i % 2 == 0 ? "Run" : "Ride",
                Distance = 5000 + (i * 1000),
                Duration = 1800 + (i * 300),
                Calories = 350 + (i * 50)
            });
        }

        return request;
    }

    /// <summary>
    /// Erstellt eine gRPC PerformanceTrendsRequest für Tests
    /// </summary>
    public static global::Fitnessanalyticshub.PerformanceTrendsRequest CreateGrpcPerformanceTrendsRequest(
        int athleteId = 123,
        string timeFrame = "month")
    {
        return new global::Fitnessanalyticshub.PerformanceTrendsRequest
        {
            AthleteId = athleteId,
            TimeFrame = timeFrame
        };
    }

    /// <summary>
    /// Erstellt eine gRPC TrainingRecommendationsRequest für Tests
    /// </summary>
    public static global::Fitnessanalyticshub.TrainingRecommendationsRequest CreateGrpcTrainingRecommendationsRequest(
        int athleteId = 456)
    {
        return new global::Fitnessanalyticshub.TrainingRecommendationsRequest
        {
            AthleteId = athleteId
        };
    }

    /// <summary>
    /// Erstellt eine gRPC HealthAnalysisRequest für Tests
    /// </summary>
    public static global::Fitnessanalyticshub.HealthAnalysisRequest CreateGrpcHealthAnalysisRequest(
        int athleteId = 789,
        int workoutCount = 3)
    {
        var request = new global::Fitnessanalyticshub.HealthAnalysisRequest
        {
            AthleteId = athleteId
        };

        for (int i = 0; i < workoutCount; i++)
        {
            request.RecentWorkouts.Add(new global::Fitnessanalyticshub.Workout
            {
                Date = DateTime.Now.AddDays(-(i + 1)).ToString("yyyy-MM-dd"),
                ActivityType = "Run",
                Distance = 5000,
                Duration = 1800,
                Calories = 350
            });
        }

        return request;
    }

    #endregion

    #region Service Response Builders

    /// <summary>
    /// Erstellt eine Standard MotivationResponseDto für Tests
    /// </summary>
    public static MotivationResponseDto CreateTestMotivationResponse(
        string motivationalMessage = "You're doing great!",
        string? quote = "Success is a journey, not a destination.",
        List<string>? actionableTips = null)
    {
        return new MotivationResponseDto
        {
            MotivationalMessage = motivationalMessage,
            Quote = quote,
            ActionableTips = actionableTips ?? new List<string>
            {
                "Set realistic goals",
                "Stay consistent",
                "Celebrate progress"
            },
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Erstellt eine Standard WorkoutAnalysisResponseDto für Tests
    /// </summary>
    public static WorkoutAnalysisResponseDto CreateTestWorkoutAnalysisResponse(
        string provider = "HuggingFace",
        string analysis = "Great workout performance!",
        List<string>? keyInsights = null,
        List<string>? recommendations = null)
    {
        return new WorkoutAnalysisResponseDto
        {
            Analysis = analysis,
            KeyInsights = keyInsights ?? new List<string>
            {
                "Consistent training pattern",
                "Good cardiovascular endurance",
                "Improving pace over time"
            },
            Recommendations = recommendations ?? new List<string>
            {
                "Maintain current training frequency",
                "Add strength training sessions",
                "Focus on recovery between workouts"
            },
            Provider = provider,
            GeneratedAt = DateTime.UtcNow,
            RequestId = Guid.NewGuid().ToString()
        };
    }

    #endregion

    #region gRPC Context Helpers

    /// <summary>
    /// Erstellt einen Mock ServerCallContext für gRPC Tests
    /// </summary>
    public static ServerCallContext CreateMockServerCallContext()
    {
        var mockContext = new Mock<ServerCallContext>();

        // Setup basic properties that might be needed
        mockContext.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        mockContext.Setup(c => c.Peer).Returns("test-peer");
        mockContext.Setup(c => c.Method).Returns("TestMethod");

        return mockContext.Object;
    }

    /// <summary>
    /// Erstellt einen Mock ServerCallContext mit Cancellation Token
    /// </summary>
    public static ServerCallContext CreateMockServerCallContextWithCancellation(
        CancellationToken cancellationToken)
    {
        var mockContext = new Mock<ServerCallContext>();
        mockContext.Setup(c => c.CancellationToken).Returns(cancellationToken);
        return mockContext.Object;
    }

    #endregion

    #region Assertion Helpers für gRPC

    /// <summary>
    /// Verifiziert, dass eine gRPC MotivationResponse korrekte Werte hat
    /// </summary>
    public static void AssertValidMotivationResponse(
        global::Fitnessanalyticshub.MotivationResponse response,
        string? expectedMessage = null,
        int? expectedTipsCount = null)
    {
        Assert.NotNull(response);
        Assert.NotNull(response.MotivationalMessage);
        Assert.False(string.IsNullOrEmpty(response.GeneratedAt));

        if (expectedMessage != null)
        {
            Assert.Equal(expectedMessage, response.MotivationalMessage);
        }

        if (expectedTipsCount.HasValue)
        {
            Assert.Equal(expectedTipsCount.Value, response.ActionableTips.Count);
        }

        // Verifiziere, dass GeneratedAt ein gültiges Datum ist
        Assert.True(DateTime.TryParse(response.GeneratedAt, out _));
    }

    /// <summary>
    /// Verifiziert, dass eine gRPC WorkoutAnalysisResponse korrekte Werte hat
    /// </summary>
    public static void AssertValidWorkoutAnalysisResponse(
        global::Fitnessanalyticshub.WorkoutAnalysisResponse response,
        string? expectedSource = null,
        string? expectedAnalysisType = null)
    {
        Assert.NotNull(response);
        Assert.NotNull(response.Analysis);
        Assert.False(string.IsNullOrEmpty(response.GeneratedAt));

        if (expectedSource != null)
        {
            Assert.Equal(expectedSource, response.Source);
        }

        if (expectedAnalysisType != null)
        {
            Assert.Equal(expectedAnalysisType, response.AnalysisType);
        }

        // Verifiziere, dass GeneratedAt ein gültiges Datum ist
        Assert.True(DateTime.TryParse(response.GeneratedAt, out _));
    }

    /// <summary>
    /// Verifiziert, dass eine gRPC HealthCheckResponse korrekte Werte hat
    /// </summary>
    public static void AssertValidHealthCheckResponse(
        global::Fitnessanalyticshub.HealthCheckResponse response,
        bool expectedHealthy = true)
    {
        Assert.NotNull(response);
        Assert.Equal(expectedHealthy, response.IsHealthy);
        Assert.NotNull(response.Message);
        Assert.False(string.IsNullOrEmpty(response.Timestamp));

        // Verifiziere, dass Timestamp ein gültiges Datum ist
        Assert.True(DateTime.TryParse(response.Timestamp, out _));
    }

    #endregion

    #region Error Testing Helpers

    /// <summary>
    /// Verifiziert, dass eine RpcException mit erwarteten Properties geworfen wurde
    /// </summary>
    public static void AssertRpcExceptionThrown(
        Func<Task> action,
        StatusCode expectedStatusCode = StatusCode.Internal,
        string? expectedMessageContains = null)
    {
        var exception = Assert.ThrowsAsync<RpcException>(action);
        Assert.Equal(expectedStatusCode, exception.Result.StatusCode);

        if (expectedMessageContains != null)
        {
            Assert.Contains(expectedMessageContains, exception.Result.Status.Detail);
        }
    }

    #endregion

    #region Integration Test Helpers

    /// <summary>
    /// Erstellt einen komplexen Test-Request für Integration Tests
    /// </summary>
    public static global::Fitnessanalyticshub.WorkoutAnalysisRequest CreateComplexWorkoutAnalysisRequest()
    {
        var request = new global::Fitnessanalyticshub.WorkoutAnalysisRequest
        {
            PreferredAiProvider = "googlegemini",
            AnalysisType = "Performance"
        };

        // Verschiedene Workout-Typen
        request.RecentWorkouts.Add(new global::Fitnessanalyticshub.Workout
        {
            Date = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"),
            ActivityType = "Run",
            Distance = 10000, // 10km
            Duration = 3000,  // 50 minutes
            Calories = 600
        });

        request.RecentWorkouts.Add(new global::Fitnessanalyticshub.Workout
        {
            Date = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd"),
            ActivityType = "Ride",
            Distance = 40000, // 40km
            Duration = 7200,  // 2 hours
            Calories = 1200
        });

        request.RecentWorkouts.Add(new global::Fitnessanalyticshub.Workout
        {
            Date = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd"),
            ActivityType = "Swim",
            Distance = 2000,  // 2km
            Duration = 3600,  // 1 hour
            Calories = 500
        });

        return request;
    }

    /// <summary>
    /// Validiert, dass Demo-Daten korrekt erstellt werden
    /// </summary>
    public static void AssertValidDemoWorkouts(List<WorkoutDataDto> workouts, int expectedCount = 3)
    {
        Assert.NotNull(workouts);
        Assert.Equal(expectedCount, workouts.Count);

        foreach (var workout in workouts)
        {
            Assert.NotNull(workout.ActivityType);
            Assert.True(workout.Distance > 0);
            Assert.True(workout.Duration > 0);
            Assert.True(workout.Calories > 0);
            Assert.True(workout.Date <= DateTime.Now);
        }
    }

    /// <summary>
    /// Validiert, dass Demo-AthleteProfile korrekt erstellt wird
    /// </summary>
    public static void AssertValidDemoAthleteProfile(AthleteProfileDto profile, int expectedAthleteId)
    {
        Assert.NotNull(profile);
        Assert.Equal(expectedAthleteId.ToString(), profile.Id);
        Assert.NotNull(profile.Name);
        Assert.NotNull(profile.FitnessLevel);
        Assert.NotNull(profile.PrimaryGoal);
        Assert.NotNull(profile.Preferences);
        Assert.True(profile.Preferences.Count > 0);
    }

    #endregion
}
