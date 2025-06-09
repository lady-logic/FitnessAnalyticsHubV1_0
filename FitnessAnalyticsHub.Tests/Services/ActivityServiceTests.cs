using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Application.Mapping;
using FitnessAnalyticsHub.Application.Services;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Exceptions.Activities;
using FitnessAnalyticsHub.Domain.Exceptions.Athletes;
using FitnessAnalyticsHub.Domain.Interfaces;
using Moq;
using Activity = FitnessAnalyticsHub.Domain.Entities.Activity;

namespace FitnessAnalyticsHub.Tests.Services;

public class ActivityServiceTests
{
    private readonly Mock<IRepository<Activity>> _mockActivityRepository;
    private readonly Mock<IRepository<Athlete>> _mockAthleteRepository;
    private readonly Mock<IStravaService> _mockStravaService;
    private readonly Mock<IAIAssistantClient> _mockAiAssistantClient;
    private readonly IMapper _mapper;
    private readonly ActivityService _activityService;

    public ActivityServiceTests()
    {
        _mockActivityRepository = new Mock<IRepository<Activity>>();
        _mockAthleteRepository = new Mock<IRepository<Athlete>>();
        _mockStravaService = new Mock<IStravaService>();
        _mockAiAssistantClient = new Mock<IAIAssistantClient>();

        // Konfiguriere AutoMapper mit dem tatsächlichen Mappingprofil
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _activityService = new ActivityService(
            _mockActivityRepository.Object,
            _mockAthleteRepository.Object,
            _mockStravaService.Object,
            _mockAiAssistantClient.Object,
            _mapper);
    }

    [Fact]
    public async Task GetActivityByIdAsync_ShouldReturnActivity_WhenActivityExists()
    {
        // Arrange
        var activityId = 1;
        var activity = new Activity
        {
            Id = activityId,
            AthleteId = 1,
            Name = "Morning Run",
            Distance = 5000,
            MovingTime = 1800,
            ElapsedTime = 1900,
            SportType = "Run",
            StartDate = DateTime.Now.AddDays(-1),
            StartDateLocal = DateTime.Now.AddDays(-1),
            Athlete = new Athlete { FirstName = "Max", LastName = "Mustermann" }
        };

        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

        // Act
        var result = await _activityService.GetActivityByIdAsync(activityId, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(activityId, result.Id);
        Assert.Equal("Morning Run", result.Name);
        Assert.Equal(5000, result.Distance);
        Assert.Equal("Run", result.SportType);
        Assert.Equal("Max Mustermann", result.AthleteFullName);
    }

    [Fact]
    public async Task GetActivityByIdAsync_ShouldThrowActivityNotFoundException_WhenActivityDoesNotExist()
    {
        // Arrange
        var activityId = 999;
        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ActivityNotFoundException>(
            () => _activityService.GetActivityByIdAsync(activityId, It.IsAny<CancellationToken>()));

        Assert.Equal(activityId, exception.ActivityId);
    }

    [Fact]
    public async Task GetActivitiesByAthleteIdAsync_ShouldReturnAthleteActivities()
    {
        // Arrange
        var athleteId = 1;
        var athlete = new Athlete
        {
            Id = athleteId,
            FirstName = "Max",
            LastName = "Mustermann"
        };

        var activities = new List<Activity>
        {
            new Activity
            {
                Id = 1,
                AthleteId = athleteId,
                Name = "Morning Run",
                Distance = 5000,
                MovingTime = 1800,
                ElapsedTime = 1900,
                SportType = "Run",
                StartDate = DateTime.Now.AddDays(-1),
                StartDateLocal = DateTime.Now.AddDays(-1)
            },
            new Activity
            {
                Id = 2,
                AthleteId = athleteId,
                Name = "Evening Ride",
                Distance = 20000,
                MovingTime = 3600,
                ElapsedTime = 3700,
                SportType = "Ride",
                StartDate = DateTime.Now.AddDays(-2),
                StartDateLocal = DateTime.Now.AddDays(-2)
            }
        };

        _mockActivityRepository.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Activity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(athlete);

        // Act
        var result = await _activityService.GetActivitiesByAthleteIdAsync(athleteId, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Morning Run", result.ElementAt(0).Name);
        Assert.Equal("Evening Ride", result.ElementAt(1).Name);
        Assert.Equal("Max Mustermann", result.ElementAt(0).AthleteFullName);
        Assert.Equal("Max Mustermann", result.ElementAt(1).AthleteFullName);
    }

    [Fact]
    public async Task CreateActivityAsync_ShouldCreateAndReturnActivity()
    {
        // Arrange
        var athleteId = 1;
        var createActivityDto = new CreateActivityDto
        {
            AthleteId = athleteId,
            Name = "Test Activity",
            Distance = 10000,
            MovingTimeSeconds = 2400,
            ElapsedTimeSeconds = 2500,
            SportType = "Run",
            StartDate = DateTime.Now,
            StartDateLocal = DateTime.Now
        };

        var athlete = new Athlete
        {
            Id = athleteId,
            FirstName = "Max",
            LastName = "Mustermann"
        };

        Activity createdActivity = null;

        _mockActivityRepository.Setup(repo => repo.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
            .Callback<Activity, CancellationToken>((activity, token) =>
            {
                activity.Id = 3; // Simuliere Datenbankgenerierung der ID
                createdActivity = activity;
            });

        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(athlete);

        // Act
        var result = await _activityService.CreateActivityAsync(createActivityDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Test Activity", result.Name);
        Assert.Equal(10000, result.Distance);
        Assert.Equal(2400, result.MovingTime.TotalSeconds);
        Assert.Equal("Max Mustermann", result.AthleteFullName);

        _mockActivityRepository.Verify(repo => repo.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockActivityRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateActivityAsync_ShouldUpdateActivity_WhenActivityExists()
    {
        // Arrange
        var updateActivityDto = new UpdateActivityDto
        {
            Id = 1,
            Name = "Updated Activity",
            Distance = 11000,
            MovingTimeSeconds = 2500,
            ElapsedTimeSeconds = 2600,
            SportType = "Run",
            StartDate = DateTime.Now,
            StartDateLocal = DateTime.Now
        };

        var existingActivity = new Activity
        {
            Id = 1,
            AthleteId = 1,
            Name = "Original Activity",
            Distance = 10000,
            MovingTime = 2400,
            ElapsedTime = 2500,
            SportType = "Run",
            StartDate = DateTime.Now.AddDays(-1),
            StartDateLocal = DateTime.Now.AddDays(-1),
            CreatedAt = DateTime.Now.AddDays(-1),
            UpdatedAt = DateTime.Now.AddDays(-1)
        };

        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(updateActivityDto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingActivity);

        // Act
        await _activityService.UpdateActivityAsync(updateActivityDto, It.IsAny<CancellationToken>());

        // Assert
        _mockActivityRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Activity>()), Times.Once);
        _mockActivityRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Überprüfen, dass die Eigenschaften korrekt aktualisiert wurden
        Assert.Equal("Updated Activity", existingActivity.Name);
        Assert.Equal(11000, existingActivity.Distance);
        Assert.Equal(2500, existingActivity.MovingTime);
    }

    [Fact]
    public async Task UpdateActivityAsync_ShouldThrowActivityNotFoundException_WhenActivityDoesNotExist()
    {
        // Arrange
        var updateActivityDto = new UpdateActivityDto
        {
            Id = 999,
            Name = "Non-existent Activity"
        };

        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(updateActivityDto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ActivityNotFoundException>(
            () => _activityService.UpdateActivityAsync(updateActivityDto, It.IsAny<CancellationToken>()));

        Assert.Equal(999, exception.ActivityId);
        _mockActivityRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Activity>()), Times.Never);
    }

    [Fact]
    public async Task DeleteActivityAsync_ShouldDeleteActivity_WhenActivityExists()
    {
        // Arrange
        var activityId = 1;
        var existingActivity = new Activity { Id = activityId };

        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingActivity);

        // Act
        await _activityService.DeleteActivityAsync(activityId, It.IsAny<CancellationToken>());

        // Assert
        _mockActivityRepository.Verify(repo => repo.DeleteAsync(existingActivity), Times.Once);
        _mockActivityRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteActivityAsync_ShouldThrowActivityNotFoundException_WhenActivityDoesNotExist()
    {
        // Arrange
        var activityId = 999;
        _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ActivityNotFoundException>(
            () => _activityService.DeleteActivityAsync(activityId, It.IsAny<CancellationToken>()));

        Assert.Equal(999, exception.ActivityId);
        _mockActivityRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Activity>()), Times.Never);
    }

    [Fact]
    public async Task GetAthleteActivityStatisticsAsync_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var athleteId = 1;
        var athlete = new Athlete { Id = athleteId, FirstName = "Max", LastName = "Mustermann" };

        var activities = new List<Activity>
            {
                new Activity
                {
                    Id = 1,
                    AthleteId = athleteId,
                    Name = "Morning Run",
                    Distance = 5000,
                    MovingTime = 1800,
                    ElapsedTime = 1900,
                    TotalElevationGain = 100,
                    SportType = "Run",
                    StartDate = new DateTime(2023, 1, 15),
                    StartDateLocal = new DateTime(2023, 1, 15)
                },
                new Activity
                {
                    Id = 2,
                    AthleteId = athleteId,
                    Name = "Evening Ride",
                    Distance = 20000,
                    MovingTime = 3600,
                    ElapsedTime = 3700,
                    TotalElevationGain = 300,
                    SportType = "Ride",
                    StartDate = new DateTime(2023, 2, 15),
                    StartDateLocal = new DateTime(2023, 2, 15)
                },
                new Activity
                {
                    Id = 3,
                    AthleteId = athleteId,
                    Name = "Weekend Run",
                    Distance = 10000,
                    MovingTime = 3000,
                    ElapsedTime = 3100,
                    TotalElevationGain = 150,
                    SportType = "Run",
                    StartDate = new DateTime(2023, 2, 20),
                    StartDateLocal = new DateTime(2023, 2, 20)
                }
            };

        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(athlete);

        _mockActivityRepository.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Activity, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(activities);

        // Act
        var result = await _activityService.GetAthleteActivityStatisticsAsync(athleteId, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.TotalActivities);
        Assert.Equal(35000, result.TotalDistance); // 5000 + 20000 + 10000
        Assert.Equal(8400, result.TotalDuration.TotalSeconds); // 1800 + 3600 + 3000
        Assert.Equal(550, result.TotalElevationGain); // 100 + 300 + 150

        // Überprüfe die Aktivitäten nach Typ
        Assert.True(result.ActivitiesByType.ContainsKey("Run"));
        Assert.Equal(2, result.ActivitiesByType["Run"]);
        Assert.True(result.ActivitiesByType.ContainsKey("Ride"));
        Assert.Equal(1, result.ActivitiesByType["Ride"]);

        // Überprüfe die Aktivitäten nach Monat
        Assert.True(result.ActivitiesByMonth.ContainsKey(1));
        Assert.Equal(1, result.ActivitiesByMonth[1]); // Januar
        Assert.True(result.ActivitiesByMonth.ContainsKey(2));
        Assert.Equal(2, result.ActivitiesByMonth[2]); // Februar
    }

    [Fact]
    public async Task GetAthleteActivityStatisticsAsync_ShouldThrowAthleteNotFoundException_WhenAthleteDoesNotExist()
    {
        // Arrange
        var athleteId = 999;
        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Athlete)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AthleteNotFoundException>(
            () => _activityService.GetAthleteActivityStatisticsAsync(athleteId, It.IsAny<CancellationToken>()));

        Assert.Equal(999, exception.AthleteId);
    }
}
