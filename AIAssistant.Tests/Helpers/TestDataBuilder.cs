using AIAssistant.Application.DTOs;
using AIAssistant.Applications.DTOs;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;

namespace AIAssistant.Tests.Helpers;

/// <summary>
/// Builder class for creating test data objects with sensible defaults.
/// Supports fluent API for customizing test data.
/// </summary>
public class TestDataBuilder
{
    public static AthleteProfileDtoBuilder AthleteProfile() => new();
    public static WorkoutDataDtoBuilder WorkoutData() => new();
    public static MotivationRequestDtoBuilder MotivationRequest() => new();
    public static MotivationResponseDtoBuilder MotivationResponse() => new();
    public static WorkoutAnalysisRequestDtoBuilder WorkoutAnalysisRequest() => new();
    public static WorkoutAnalysisResponseDtoBuilder WorkoutAnalysisResponse() => new();
    public static GrpcJsonWorkoutDtoBuilder GrpcJsonWorkout() => new();
    public static GrpcJsonAthleteProfileDtoBuilder GrpcJsonAthleteProfile() => new();
}

#region AthleteProfile Builder

public class AthleteProfileDtoBuilder
{
    private AthleteProfileDto _dto = new()
    {
        Id = "1",
        Name = "Test Athlete",
        FitnessLevel = "Intermediate",
        PrimaryGoal = "General Fitness",
        Preferences = new Dictionary<string, object>
        {
            { "preferredActivities", new[] { "Run", "Ride" } },
            { "trainingDays", 4 }
        }
    };

    public AthleteProfileDtoBuilder WithId(string id)
    {
        _dto.Id = id;
        return this;
    }

    public AthleteProfileDtoBuilder WithName(string name)
    {
        _dto.Name = name;
        return this;
    }

    public AthleteProfileDtoBuilder WithFitnessLevel(string fitnessLevel)
    {
        _dto.FitnessLevel = fitnessLevel;
        return this;
    }

    public AthleteProfileDtoBuilder WithPrimaryGoal(string primaryGoal)
    {
        _dto.PrimaryGoal = primaryGoal;
        return this;
    }

    public AthleteProfileDtoBuilder Beginner()
    {
        _dto.FitnessLevel = "Beginner";
        _dto.PrimaryGoal = "Get Started";
        return this;
    }

    public AthleteProfileDtoBuilder Advanced()
    {
        _dto.FitnessLevel = "Advanced";
        _dto.PrimaryGoal = "Performance Improvement";
        return this;
    }

    public AthleteProfileDto Build() => _dto;
}

#endregion

#region WorkoutData Builder

public class WorkoutDataDtoBuilder
{
    private WorkoutDataDto _dto = new()
    {
        Date = DateTime.UtcNow.AddDays(-1),
        ActivityType = "Run",
        Distance = 5000,
        Duration = 1800, // 30 minutes
        Calories = 350,
        MetricsData = new Dictionary<string, double>
        {
            { "heartRate", 145 },
            { "pace", 6.0 }
        }
    };

    public WorkoutDataDtoBuilder WithDate(DateTime date)
    {
        _dto.Date = date;
        return this;
    }

    public WorkoutDataDtoBuilder WithActivityType(string activityType)
    {
        _dto.ActivityType = activityType;
        return this;
    }

    public WorkoutDataDtoBuilder WithDistance(double distance)
    {
        _dto.Distance = distance;
        return this;
    }

    public WorkoutDataDtoBuilder WithDuration(int durationSeconds)
    {
        _dto.Duration = durationSeconds;
        return this;
    }

    public WorkoutDataDtoBuilder WithCalories(int? calories)
    {
        _dto.Calories = calories;
        return this;
    }

    public WorkoutDataDtoBuilder AsRun(double distanceKm = 5.0, int durationMinutes = 30)
    {
        _dto.ActivityType = "Run";
        _dto.Distance = distanceKm * 1000; // Convert to meters
        _dto.Duration = durationMinutes * 60; // Convert to seconds
        _dto.Calories = (int)(distanceKm * 70); // Rough calorie estimate
        return this;
    }

    public WorkoutDataDtoBuilder AsRide(double distanceKm = 20.0, int durationMinutes = 60)
    {
        _dto.ActivityType = "Ride";
        _dto.Distance = distanceKm * 1000;
        _dto.Duration = durationMinutes * 60;
        _dto.Calories = (int)(distanceKm * 40);
        return this;
    }

    public WorkoutDataDtoBuilder AsSwim(double distanceM = 1000, int durationMinutes = 30)
    {
        _dto.ActivityType = "Swim";
        _dto.Distance = distanceM;
        _dto.Duration = durationMinutes * 60;
        _dto.Calories = (int)(distanceM * 0.8);
        return this;
    }

    public WorkoutDataDto Build() => _dto;
}

#endregion

#region MotivationRequest Builder

public class MotivationRequestDtoBuilder
{
    private MotivationRequestDto _dto = new()
    {
        AthleteProfile = TestDataBuilder.AthleteProfile().Build(),
        IsStruggling = false,
        UpcomingWorkoutType = "Run",
        LastWorkout = TestDataBuilder.WorkoutData().Build()
    };

    public MotivationRequestDtoBuilder WithAthleteProfile(AthleteProfileDto profile)
    {
        _dto.AthleteProfile = profile;
        return this;
    }

    public MotivationRequestDtoBuilder WithStruggling(bool isStruggling)
    {
        _dto.IsStruggling = isStruggling;
        return this;
    }

    public MotivationRequestDtoBuilder WithUpcomingWorkoutType(string workoutType)
    {
        _dto.UpcomingWorkoutType = workoutType;
        return this;
    }

    public MotivationRequestDtoBuilder WithLastWorkout(WorkoutDataDto workout)
    {
        _dto.LastWorkout = workout;
        return this;
    }

    public MotivationRequestDtoBuilder Struggling()
    {
        _dto.IsStruggling = true;
        _dto.AthleteProfile = TestDataBuilder.AthleteProfile()
            .WithName("Struggling Athlete")
            .WithFitnessLevel("Beginner")
            .Build();
        return this;
    }

    public MotivationRequestDtoBuilder Motivated()
    {
        _dto.IsStruggling = false;
        _dto.AthleteProfile = TestDataBuilder.AthleteProfile()
            .WithName("Motivated Athlete")
            .Advanced()
            .Build();
        return this;
    }

    public MotivationRequestDto Build() => _dto;
}

#endregion

#region MotivationResponse Builder

public class MotivationResponseDtoBuilder
{
    private MotivationResponseDto _dto = new()
    {
        MotivationalMessage = "You're doing great! Keep pushing forward!",
        Quote = "Success is the sum of small efforts repeated day in and day out.",
        ActionableTips = new List<string>
        {
            "Focus on consistency over perfection",
            "Celebrate small victories",
            "Set realistic daily goals"
        },
        GeneratedAt = DateTime.UtcNow
    };

    public MotivationResponseDtoBuilder WithMessage(string message)
    {
        _dto.MotivationalMessage = message;
        return this;
    }

    public MotivationResponseDtoBuilder WithQuote(string quote)
    {
        _dto.Quote = quote;
        return this;
    }

    public MotivationResponseDtoBuilder WithTips(params string[] tips)
    {
        _dto.ActionableTips = tips.ToList();
        return this;
    }

    public MotivationResponseDtoBuilder WithGeneratedAt(DateTime generatedAt)
    {
        _dto.GeneratedAt = generatedAt;
        return this;
    }

    public MotivationResponseDtoBuilder ForStruggling()
    {
        _dto.MotivationalMessage = "It's okay to struggle - that's how we grow stronger! Every step forward counts.";
        _dto.Quote = "The strongest people are those who win battles we know nothing about.";
        _dto.ActionableTips = new List<string>
        {
            "Start with just 10 minutes of activity",
            "Remember why you started",
            "Ask for support when you need it"
        };
        return this;
    }

    public MotivationResponseDtoBuilder ForAdvanced()
    {
        _dto.MotivationalMessage = "Champion mindset! You're training for excellence.";
        _dto.Quote = "Champions are made in the gym, legends are made through dedication.";
        _dto.ActionableTips = new List<string>
        {
            "Visualize your competition success",
            "Focus on technique perfection",
            "Trust your training process"
        };
        return this;
    }

    public MotivationResponseDto Build() => _dto;
}

#endregion

#region WorkoutAnalysisRequest Builder

public class WorkoutAnalysisRequestDtoBuilder
{
    private WorkoutAnalysisRequestDto _dto = new()
    {
        RecentWorkouts = new List<WorkoutDataDto>
        {
            TestDataBuilder.WorkoutData().AsRun().Build(),
            TestDataBuilder.WorkoutData().AsRide().WithDate(DateTime.UtcNow.AddDays(-2)).Build()
        },
        AnalysisType = "Performance",
        AthleteProfile = TestDataBuilder.AthleteProfile().Build(),
        AdditionalContext = new Dictionary<string, object>
        {
            { "focus", "endurance" },
            { "timeframe", "week" }
        }
    };

    public WorkoutAnalysisRequestDtoBuilder WithWorkouts(params WorkoutDataDto[] workouts)
    {
        _dto.RecentWorkouts = workouts.ToList();
        return this;
    }

    public WorkoutAnalysisRequestDtoBuilder WithAnalysisType(string analysisType)
    {
        _dto.AnalysisType = analysisType;
        return this;
    }

    public WorkoutAnalysisRequestDtoBuilder WithAthleteProfile(AthleteProfileDto profile)
    {
        _dto.AthleteProfile = profile;
        return this;
    }

    public WorkoutAnalysisRequestDtoBuilder ForPerformance()
    {
        _dto.AnalysisType = "Performance";
        _dto.AdditionalContext = new Dictionary<string, object>
        {
            { "focus", "speed_improvement" }
        };
        return this;
    }

    public WorkoutAnalysisRequestDtoBuilder ForHealth()
    {
        _dto.AnalysisType = "Health";
        _dto.AdditionalContext = new Dictionary<string, object>
        {
            { "focus", "injury_prevention" }
        };
        return this;
    }

    public WorkoutAnalysisRequestDtoBuilder ForTrends()
    {
        _dto.AnalysisType = "Trends";
        _dto.AdditionalContext = new Dictionary<string, object>
        {
            { "timeframe", "month" }
        };
        return this;
    }

    public WorkoutAnalysisRequestDto Build() => _dto;
}

#endregion

#region WorkoutAnalysisResponse Builder

public class WorkoutAnalysisResponseDtoBuilder
{
    private WorkoutAnalysisResponseDto _dto = new()
    {
        Analysis = "Your recent workouts show excellent progression and consistent training patterns.",
        KeyInsights = new List<string>
        {
            "Training consistency is strong with 4 workouts per week",
            "Average pace has improved by 10 seconds per kilometer",
            "Heart rate zones indicate optimal training intensity"
        },
        Recommendations = new List<string>
        {
            "Continue current training frequency",
            "Add one tempo run per week for speed development",
            "Focus on recovery between high-intensity sessions"
        },
        Provider = "GoogleGemini",
        RequestId = Guid.NewGuid().ToString(),
        GeneratedAt = DateTime.UtcNow
    };

    public WorkoutAnalysisResponseDtoBuilder WithAnalysis(string analysis)
    {
        _dto.Analysis = analysis;
        return this;
    }

    public WorkoutAnalysisResponseDtoBuilder WithInsights(params string[] insights)
    {
        _dto.KeyInsights = insights.ToList();
        return this;
    }

    public WorkoutAnalysisResponseDtoBuilder WithRecommendations(params string[] recommendations)
    {
        _dto.Recommendations = recommendations.ToList();
        return this;
    }

    public WorkoutAnalysisResponseDtoBuilder WithProvider(string provider)
    {
        _dto.Provider = provider;
        return this;
    }

    public WorkoutAnalysisResponseDto Build() => _dto;
}

#endregion

#region GrpcJson Builders

public class GrpcJsonWorkoutDtoBuilder
{
    private GrpcJsonWorkoutDto _dto = new()
    {
        Date = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd"),
        ActivityType = "Run",
        Distance = 5000,
        Duration = 1800,
        Calories = 350
    };

    public GrpcJsonWorkoutDtoBuilder WithDate(string date)
    {
        _dto.Date = date;
        return this;
    }

    public GrpcJsonWorkoutDtoBuilder WithActivityType(string activityType)
    {
        _dto.ActivityType = activityType;
        return this;
    }

    public GrpcJsonWorkoutDto Build() => _dto;
}

public class GrpcJsonAthleteProfileDtoBuilder
{
    private GrpcJsonAthleteProfileDto _dto = new()
    {
        Name = "Test Athlete",
        FitnessLevel = "Intermediate",
        PrimaryGoal = "General Fitness"
    };

    public GrpcJsonAthleteProfileDtoBuilder WithName(string name)
    {
        _dto.Name = name;
        return this;
    }

    public GrpcJsonAthleteProfileDto Build() => _dto;
}

#endregion