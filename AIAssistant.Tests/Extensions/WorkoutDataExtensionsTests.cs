namespace AIAssistant.Tests.Extensions;

using AIAssistant.Application.DTOs;
using AIAssistant.Domain.Models;
using AIAssistant.Tests.Helpers;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using FitnessAnalyticsHub.AIAssistant.Extensions;

public class WorkoutDataExtensionsTests
{
    #region ToDomain Tests

    [Fact]
    public void ToDomain_WithValidDto_ReturnsCorrectDomainModel()
    {
        // Arrange
        WorkoutDataDto dto = TestDataBuilder.WorkoutData()
            .AsRun(10.0, 45)
            .WithCalories(500)
            .Build();

        // Act
        WorkoutData domain = dto.ToDomain();

        // Assert
        Assert.NotNull(domain);
        Assert.Equal(dto.Date, domain.Date);
        Assert.Equal(dto.ActivityType, domain.ActivityType);
        Assert.Equal(dto.Distance, domain.Distance);
        Assert.Equal(dto.Duration, domain.Duration);
        Assert.Equal(dto.Calories, domain.Calories);
        Assert.Equal(dto.MetricsData, domain.MetricsData);
    }

    [Fact]
    public void ToDomain_WithNullCalories_HandlesCorrectly()
    {
        // Arrange
        WorkoutDataDto dto = TestDataBuilder.WorkoutData()
            .WithCalories(null)
            .Build();

        // Act
        WorkoutData domain = dto.ToDomain();

        // Assert
        Assert.NotNull(domain);
        Assert.Null(domain.Calories);
    }

    [Fact]
    public void ToDomain_WithComplexMetricsData_PreservesData()
    {
        // Arrange
        Dictionary<string, double> complexMetrics = new Dictionary<string, double>
        {
            { "averageHeartRate", 155.5 },
            { "maxHeartRate", 185.0 },
            { "averagePace", 4.25 }, // min/km
            { "maxPace", 3.45 },
            { "elevationGain", 125.7 },
            { "cadence", 180.0 },
            { "powerOutput", 250.8 },
        };

        WorkoutDataDto dto = new WorkoutDataDto
        {
            Date = DateTime.UtcNow,
            ActivityType = "Run",
            Distance = 15000,
            Duration = 3600,
            Calories = 750,
            MetricsData = complexMetrics,
        };

        // Act
        WorkoutData domain = dto.ToDomain();

        // Assert
        Assert.NotNull(domain);
        Assert.Equal(complexMetrics, domain.MetricsData);
        Assert.Equal(155.5, domain.MetricsData!["averageHeartRate"]);
        Assert.Equal(250.8, domain.MetricsData["powerOutput"]);
    }

    #endregion

    #region ToDto Tests

    [Fact]
    public void ToDto_WithValidDomainModel_ReturnsCorrectDto()
    {
        // Arrange
        WorkoutData domain = new WorkoutData
        {
            Date = DateTime.UtcNow.AddDays(-1),
            ActivityType = "Swim",
            Distance = 2000,
            Duration = 1800, // 30 minutes
            Calories = 400,
            MetricsData = new Dictionary<string, double>
            {
                { "strokeRate", 30 },
                { "pace", 1.5 },
            },
        };

        // Act
        WorkoutDataDto dto = domain.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(domain.Date, dto.Date);
        Assert.Equal(domain.ActivityType, dto.ActivityType);
        Assert.Equal(domain.Distance, dto.Distance);
        Assert.Equal(domain.Duration, dto.Duration);
        Assert.Equal(domain.Calories, dto.Calories);
        Assert.Equal(domain.MetricsData, dto.MetricsData);
    }

    [Fact]
    public void ToDto_WithNullMetricsData_HandlesCorrectly()
    {
        // Arrange
        WorkoutData domain = new WorkoutData
        {
            Date = DateTime.UtcNow,
            ActivityType = "Walk",
            Distance = 3000,
            Duration = 1800,
            Calories = 200,
            MetricsData = null,
        };

        // Act
        WorkoutDataDto dto = domain.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.MetricsData);
    }

    #endregion

    #region gRPC Workout ToWorkoutDataDto Tests

    [Fact]
    public void ToWorkoutDataDto_WithValidGrpcWorkout_ReturnsCorrectDto()
    {
        // Arrange
        Fitnessanalyticshub.Workout grpcWorkout = new global::Fitnessanalyticshub.Workout
        {
            Date = "2024-01-15",
            ActivityType = "Run",
            Distance = 5000,
            Duration = 1800,
            Calories = 350,
        };

        // Act
        WorkoutDataDto dto = grpcWorkout.ToWorkoutDataDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(new DateTime(2024, 1, 15), dto.Date);
        Assert.Equal("Run", dto.ActivityType);
        Assert.Equal(5000, dto.Distance);
        Assert.Equal(1800, dto.Duration);
        Assert.Equal(350, dto.Calories);
        Assert.Null(dto.MetricsData); // gRPC doesn't have metrics data
    }

    [Fact]
    public void ToWorkoutDataDto_WithInvalidDate_UsesCurrentTime()
    {
        // Arrange
        Fitnessanalyticshub.Workout grpcWorkout = new global::Fitnessanalyticshub.Workout
        {
            Date = "invalid-date",
            ActivityType = "Run",
            Distance = 5000,
            Duration = 1800,
            Calories = 350,
        };

        // Act
        WorkoutDataDto dto = grpcWorkout.ToWorkoutDataDto();

        // Assert
        Assert.NotNull(dto);
        Assert.True(dto.Date > DateTime.UtcNow.AddMinutes(-1)); // Should be recent
        Assert.Equal("Run", dto.ActivityType);
    }

    [Fact]
    public void ToWorkoutDataDto_WithDefaultValues_HandlesGracefully()
    {
        // Arrange
        Fitnessanalyticshub.Workout grpcWorkout = new global::Fitnessanalyticshub.Workout();

        // Alle Properties haben gRPC default values

        // Act
        WorkoutDataDto dto = grpcWorkout.ToWorkoutDataDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(string.Empty, dto.ActivityType); // gRPC default für string
        Assert.Equal(0, dto.Distance);      // gRPC default für int/double
        Assert.Equal(0, dto.Duration);
        Assert.Equal(0, dto.Calories);
    }

    [Theory]
    [InlineData("2024-01-15T10:30:00")]
    [InlineData("2024-01-15T10:30:00Z")]
    [InlineData("2024-01-15")]
    [InlineData("2024/01/15")]
    public void ToWorkoutDataDto_WithVariousDateFormats_ParsesCorrectly(string dateString)
    {
        // Arrange
        Fitnessanalyticshub.Workout grpcWorkout = new global::Fitnessanalyticshub.Workout
        {
            Date = dateString,
            ActivityType = "Test",
            Distance = 1000,
            Duration = 600,
            Calories = 100,
        };

        // Act
        WorkoutDataDto dto = grpcWorkout.ToWorkoutDataDto();

        // Assert
        Assert.NotNull(dto);
        Assert.True(dto.Date.Year == 2024);
        Assert.True(dto.Date.Month == 1);
        Assert.True(dto.Date.Day == 15);
    }

    #endregion

    #region GrpcJsonWorkout ToWorkoutDataDto Tests

    [Fact]
    public void ToWorkoutDataDto_WithValidGrpcJsonWorkout_ReturnsCorrectDto()
    {
        // Arrange
        GrpcJsonWorkoutDto grpcJsonWorkout = TestDataBuilder.GrpcJsonWorkout()
            .WithDate("2024-02-20")
            .WithActivityType("Ride")
            .Build();
        grpcJsonWorkout.Distance = 25000;
        grpcJsonWorkout.Duration = 3600;
        grpcJsonWorkout.Calories = 800;

        // Act
        WorkoutDataDto dto = grpcJsonWorkout.ToWorkoutDataDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(new DateTime(2024, 2, 20), dto.Date);
        Assert.Equal("Ride", dto.ActivityType);
        Assert.Equal(25000, dto.Distance);
        Assert.Equal(3600, dto.Duration);
        Assert.Equal(800, dto.Calories);
    }

    [Fact]
    public void ToWorkoutDataDto_WithInvalidGrpcJsonDate_UsesCurrentTime()
    {
        // Arrange
        GrpcJsonWorkoutDto grpcJsonWorkout = new GrpcJsonWorkoutDto
        {
            Date = "not-a-date",
            ActivityType = "Run",
            Distance = 5000,
            Duration = 1800,
            Calories = 350,
        };

        // Act
        WorkoutDataDto dto = grpcJsonWorkout.ToWorkoutDataDto();

        // Assert
        Assert.NotNull(dto);
        Assert.True(dto.Date > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void ToWorkoutDataDto_WithEmptyGrpcJsonDate_UsesCurrentTime()
    {
        // Arrange
        GrpcJsonWorkoutDto grpcJsonWorkout = new GrpcJsonWorkoutDto
        {
            Date = string.Empty,
            ActivityType = "Swim",
            Distance = 1000,
            Duration = 1200,
            Calories = 200,
        };

        // Act
        WorkoutDataDto dto = grpcJsonWorkout.ToWorkoutDataDto();

        // Assert
        Assert.NotNull(dto);
        Assert.True(dto.Date > DateTime.UtcNow.AddMinutes(-1));
        Assert.Equal("Swim", dto.ActivityType);
    }

    #endregion

    #region ToWorkoutAnalysisRequestDto Tests

    [Fact]
    public void ToWorkoutAnalysisRequestDto_WithValidGrpcJsonRequest_ReturnsCorrectDto()
    {
        // Arrange
        GrpcJsonWorkoutAnalysisRequestDto grpcJsonRequest = new GrpcJsonWorkoutAnalysisRequestDto
        {
            AthleteProfile = TestDataBuilder.GrpcJsonAthleteProfile()
                .WithName("Analysis Test User")
                .Build(),
            RecentWorkouts = new[]
            {
                TestDataBuilder.GrpcJsonWorkout().WithActivityType("Run").Build(),
                TestDataBuilder.GrpcJsonWorkout().WithActivityType("Ride").Build(),
            },
            AnalysisType = "Performance",
            FocusAreas = new[] { "endurance", "speed" },
            PreferredAiProvider = "GoogleGemini",
        };

        // Act
        WorkoutAnalysisRequestDto dto = grpcJsonRequest.ToWorkoutAnalysisRequestDto();

        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.AthleteProfile);
        Assert.Equal("Analysis Test User", dto.AthleteProfile.Name);
        Assert.NotNull(dto.RecentWorkouts);
        Assert.Equal(2, dto.RecentWorkouts.Count);
        Assert.Equal("Run", dto.RecentWorkouts[0].ActivityType);
        Assert.Equal("Ride", dto.RecentWorkouts[1].ActivityType);
        Assert.Equal("Performance", dto.AnalysisType);
    }

    [Fact]
    public void ToWorkoutAnalysisRequestDto_WithNullAthleteProfile_HandlesGracefully()
    {
        // Arrange
        GrpcJsonWorkoutAnalysisRequestDto grpcJsonRequest = new GrpcJsonWorkoutAnalysisRequestDto
        {
            AthleteProfile = null,
            RecentWorkouts = new[] { TestDataBuilder.GrpcJsonWorkout().Build() },
            AnalysisType = "Health",
        };

        // Act
        WorkoutAnalysisRequestDto dto = grpcJsonRequest.ToWorkoutAnalysisRequestDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.AthleteProfile);
        Assert.NotNull(dto.RecentWorkouts);
        Assert.Single(dto.RecentWorkouts);
        Assert.Equal("Health", dto.AnalysisType);
    }

    [Fact]
    public void ToWorkoutAnalysisRequestDto_WithNullWorkouts_ReturnsEmptyList()
    {
        // Arrange
        GrpcJsonWorkoutAnalysisRequestDto grpcJsonRequest = new GrpcJsonWorkoutAnalysisRequestDto
        {
            AthleteProfile = TestDataBuilder.GrpcJsonAthleteProfile().Build(),
            RecentWorkouts = null,
            AnalysisType = "Trends",
        };

        // Act
        WorkoutAnalysisRequestDto dto = grpcJsonRequest.ToWorkoutAnalysisRequestDto();

        // Assert
        Assert.NotNull(dto);
        Assert.NotNull(dto.RecentWorkouts);
        Assert.Empty(dto.RecentWorkouts);
        Assert.Equal("Trends", dto.AnalysisType);
    }

    [Fact]
    public void ToWorkoutAnalysisRequestDto_WithNullAnalysisType_UsesDefaultPerformance()
    {
        // Arrange
        GrpcJsonWorkoutAnalysisRequestDto grpcJsonRequest = new GrpcJsonWorkoutAnalysisRequestDto
        {
            AthleteProfile = TestDataBuilder.GrpcJsonAthleteProfile().Build(),
            RecentWorkouts = new[] { TestDataBuilder.GrpcJsonWorkout().Build() },
            AnalysisType = null,
        };

        // Act
        WorkoutAnalysisRequestDto dto = grpcJsonRequest.ToWorkoutAnalysisRequestDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Performance", dto.AnalysisType); // Should default to Performance
    }

    #endregion

    #region Edge Cases and Data Integrity

    [Fact]
    public void WorkoutDataExtensions_WithZeroValues_PreservesValues()
    {
        // Arrange
        WorkoutDataDto dto = new WorkoutDataDto
        {
            Date = DateTime.UtcNow,
            ActivityType = "Rest",
            Distance = 0,
            Duration = 0,
            Calories = 0,
        };

        // Act
        WorkoutData domain = dto.ToDomain();
        WorkoutDataDto backToDto = domain.ToDto();

        // Assert
        Assert.Equal(0, domain.Distance);
        Assert.Equal(0, domain.Duration);
        Assert.Equal(0, domain.Calories);
        Assert.Equal(0, backToDto.Distance);
        Assert.Equal(0, backToDto.Duration);
        Assert.Equal(0, backToDto.Calories);
    }

    [Fact]
    public void WorkoutDataExtensions_WithLargeValues_HandlesCorrectly()
    {
        // Arrange
        WorkoutDataDto dto = new WorkoutDataDto
        {
            Date = DateTime.UtcNow,
            ActivityType = "Ultra Marathon",
            Distance = 100000, // 100km
            Duration = 36000,  // 10 hours
            Calories = 5000,
        };

        // Act
        WorkoutData domain = dto.ToDomain();
        WorkoutDataDto backToDto = domain.ToDto();

        // Assert
        Assert.Equal(100000, domain.Distance);
        Assert.Equal(36000, domain.Duration);
        Assert.Equal(5000, domain.Calories);
        Assert.Equal(dto.Distance, backToDto.Distance);
        Assert.Equal(dto.Duration, backToDto.Duration);
        Assert.Equal(dto.Calories, backToDto.Calories);
    }

    [Fact]
    public void WorkoutDataExtensions_RoundTripConversion_PreservesAllData()
    {
        // Arrange
        WorkoutDataDto originalDto = TestDataBuilder.WorkoutData()
            .AsRun(21.1, 105) // Half marathon
            .WithCalories(1200)
            .WithDate(DateTime.UtcNow.AddDays(-3))
            .Build();

        originalDto.MetricsData = new Dictionary<string, double>
        {
            { "averageHeartRate", 165.5 },
            { "maxHeartRate", 185.0 },
            { "averagePace", 5.0 },
            { "elevationGain", 250.3 },
        };

        // Act
        WorkoutData domain = originalDto.ToDomain();
        WorkoutDataDto convertedDto = domain.ToDto();

        // Assert
        Assert.Equal(originalDto.Date, convertedDto.Date);
        Assert.Equal(originalDto.ActivityType, convertedDto.ActivityType);
        Assert.Equal(originalDto.Distance, convertedDto.Distance);
        Assert.Equal(originalDto.Duration, convertedDto.Duration);
        Assert.Equal(originalDto.Calories, convertedDto.Calories);
        Assert.Equal(originalDto.MetricsData, convertedDto.MetricsData);
    }

    [Theory]
    [InlineData(-1000)] // Negative distance (invalid)
    [InlineData(0)] // Zero distance (rest day)
    [InlineData(200000)] // Very long distance (200km)
    public void ToWorkoutDataDto_WithVariousDistances_HandlesCorrectly(double distance)
    {
        // Arrange
        Fitnessanalyticshub.Workout grpcWorkout = new global::Fitnessanalyticshub.Workout
        {
            Date = "2024-01-15",
            ActivityType = "Test",
            Distance = distance,
            Duration = 3600,
            Calories = 400,
        };

        // Act
        WorkoutDataDto dto = grpcWorkout.ToWorkoutDataDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(distance, dto.Distance);
    }

    [Theory]
    [InlineData(0)] // No duration
    [InlineData(60)] // 1 minute
    [InlineData(86400)] // 24 hours
    public void ToWorkoutDataDto_WithVariousDurations_HandlesCorrectly(int duration)
    {
        // Arrange
        Fitnessanalyticshub.Workout grpcWorkout = new global::Fitnessanalyticshub.Workout
        {
            Date = "2024-01-15",
            ActivityType = "Test",
            Distance = 5000,
            Duration = duration,
            Calories = 400,
        };

        // Act
        WorkoutDataDto dto = grpcWorkout.ToWorkoutDataDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(duration, dto.Duration);
    }

    #endregion
}