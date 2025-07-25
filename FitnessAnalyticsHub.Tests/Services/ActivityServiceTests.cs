﻿namespace FitnessAnalyticsHub.Tests.Services;

using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Application.Mapping;
using FitnessAnalyticsHub.Application.Services;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Exceptions.Activities;
using FitnessAnalyticsHub.Domain.Interfaces;
using FitnessAnalyticsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Activity = FitnessAnalyticsHub.Domain.Entities.Activity;

public class ActivityServiceTests
{
    private readonly ApplicationDbContext context;
    private readonly Mock<IStravaService> mockStravaService;
    private readonly Mock<IAIAssistantClientService> mockAiAssistantClient;
    private readonly IMapper mapper;
    private readonly ActivityService activityService;

    public ActivityServiceTests()
    {
        // InMemory Database erstellen
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Eindeutiger Name pro Test
            .Options;

        this.context = new ApplicationDbContext(options);
        this.mockStravaService = new Mock<IStravaService>();
        this.mockAiAssistantClient = new Mock<IAIAssistantClientService>();

        // Konfiguriere AutoMapper mit dem tatsächlichen Mappingprofil
        MapperConfiguration mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        this.mapper = mapperConfig.CreateMapper();

        // Service erstellen
        this.activityService = new ActivityService(
            this.context,
            this.mockStravaService.Object,
            this.mapper);
    }

    public void Dispose()
    {
        this.context.Dispose();
    }

    [Fact]
    public async Task GetActivityByIdAsync_ShouldReturnActivity_WhenActivityExists()
    {
        // Arrange
        Athlete athlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        Activity activity = new Activity
        {
            Id = 1,
            AthleteId = 1,
            Name = "Morning Run",
            Distance = 5000,
            MovingTime = 1800,
            ElapsedTime = 1900,
            SportType = "Run",
            StartDate = DateTime.Now.AddDays(-1),
            StartDateLocal = DateTime.Now.AddDays(-1),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        // Daten in InMemory Database einfügen
        await this.context.Athletes.AddAsync(athlete);
        await this.context.Activities.AddAsync(activity);
        await this.context.SaveChangesAsync();

        // Act
        ActivityDto result = await this.activityService.GetActivityByIdAsync(1, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Morning Run", result.Name);
        Assert.Equal(5000, result.Distance);
        Assert.Equal("Run", result.SportType);
        Assert.Equal("Max Mustermann", result.AthleteFullName);
    }

    [Fact]
    public async Task GetActivityByIdAsync_ShouldThrowActivityNotFoundException_WhenActivityDoesNotExist()
    {
        // Arrange - Keine Daten in DB einfügen

        // Act & Assert
        ActivityNotFoundException exception = await Assert.ThrowsAsync<ActivityNotFoundException>(
            () => this.activityService.GetActivityByIdAsync(999, CancellationToken.None));

        Assert.Equal(999, exception.ActivityId);
    }

    [Fact]
    public async Task GetActivitiesByAthleteIdAsync_ShouldReturnActivities_WhenActivitiesExist()
    {
        // Arrange
        Athlete athlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        List<Activity> activities = new List<Activity>
    {
        new Activity
        {
            Id = 1,
            AthleteId = 1,
            Name = "Morning Run",
            Distance = 5000,
            SportType = "Run",
            StartDate = DateTime.Now.AddDays(-2),
            StartDateLocal = DateTime.Now.AddDays(-2),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        },
        new Activity
        {
            Id = 2,
            AthleteId = 1,
            Name = "Evening Bike",
            Distance = 15000,
            SportType = "Ride",
            StartDate = DateTime.Now.AddDays(-1),
            StartDateLocal = DateTime.Now.AddDays(-1),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        },
    };

        await this.context.Athletes.AddAsync(athlete);
        await this.context.Activities.AddRangeAsync(activities);
        await this.context.SaveChangesAsync();

        // Act
        IEnumerable<ActivityDto> result = await this.activityService.GetActivitiesByAthleteIdAsync(1, CancellationToken.None);

        // Assert
        List<ActivityDto> resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.All(resultList, a => Assert.Equal("Max Mustermann", a.AthleteFullName));
        Assert.Contains(resultList, a => a.Name == "Morning Run");
        Assert.Contains(resultList, a => a.Name == "Evening Bike");
    }

    [Fact]
    public async Task CreateActivityAsync_ShouldCreateActivity_WhenValidData()
    {
        // Arrange
        Athlete athlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        await this.context.Athletes.AddAsync(athlete);
        await this.context.SaveChangesAsync();

        CreateActivityDto createDto = new CreateActivityDto
        {
            AthleteId = 1,
            Name = "Test Run",
            Distance = 5000,
            MovingTimeSeconds = 1800,
            ElapsedTimeSeconds = 1900,
            SportType = "Run",
            StartDate = DateTime.Now,
        };

        // Act
        ActivityDto result = await this.activityService.CreateActivityAsync(createDto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Run", result.Name);
        Assert.Equal(5000, result.Distance);
        Assert.True(result.Id > 0);

        // Verify in database
        Activity? activityInDb = await this.context.Activities.FindAsync(result.Id);
        Assert.NotNull(activityInDb);
        Assert.Equal("Test Run", activityInDb.Name);
    }

    [Fact]
    public async Task UpdateActivityAsync_ShouldUpdateActivity_WhenActivityExists()
    {
        // Arrange
        Athlete athlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        Activity activity = new Activity
        {
            Id = 1,
            AthleteId = 1,
            Name = "Original Name",
            Distance = 5000,
            SportType = "Run",
            StartDate = DateTime.Now,
            StartDateLocal = DateTime.Now,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        await this.context.Athletes.AddAsync(athlete);
        await this.context.Activities.AddAsync(activity);
        await this.context.SaveChangesAsync();

        UpdateActivityDto updateDto = new UpdateActivityDto
        {
            Id = 1,
            Name = "Updated Name",
            Distance = 6000,
        };

        // Act
        await this.activityService.UpdateActivityAsync(updateDto, CancellationToken.None);

        // Assert
        Activity? updatedActivity = await this.context.Activities.FindAsync(1);
        Assert.NotNull(updatedActivity);
        Assert.Equal("Updated Name", updatedActivity.Name);
        Assert.Equal(6000, updatedActivity.Distance);
    }

    [Fact]
    public async Task UpdateActivityAsync_ShouldThrowActivityNotFoundException_WhenActivityDoesNotExist()
    {
        // Arrange
        UpdateActivityDto updateDto = new UpdateActivityDto
        {
            Id = 999,
            Name = "Updated Name",
            Distance = 6000,
        };

        // Act & Assert
        ActivityNotFoundException exception = await Assert.ThrowsAsync<ActivityNotFoundException>(
            () => this.activityService.UpdateActivityAsync(updateDto, CancellationToken.None));

        Assert.Equal(999, exception.ActivityId);
    }

    [Fact]
    public async Task DeleteActivityAsync_ShouldDeleteActivity_WhenActivityExists()
    {
        // Arrange
        Athlete athlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        Activity activity = new Activity
        {
            Id = 1,
            AthleteId = 1,
            Name = "To Delete",
            SportType = "Run",
            StartDate = DateTime.Now,
            StartDateLocal = DateTime.Now,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        await this.context.Athletes.AddAsync(athlete);
        await this.context.Activities.AddAsync(activity);
        await this.context.SaveChangesAsync();

        // Act
        await this.activityService.DeleteActivityAsync(1, CancellationToken.None);

        // Assert
        Activity? deletedActivity = await this.context.Activities.FindAsync(1);
        Assert.Null(deletedActivity);
    }

    [Fact]
    public async Task DeleteActivityAsync_ShouldThrowActivityNotFoundException_WhenActivityDoesNotExist()
    {
        // Arrange - Keine Activity in DB

        // Act & Assert
        ActivityNotFoundException exception = await Assert.ThrowsAsync<ActivityNotFoundException>(
            () => this.activityService.DeleteActivityAsync(999, CancellationToken.None));

        Assert.Equal(999, exception.ActivityId);
    }

    [Fact]
    public async Task GetAthleteActivityStatisticsAsync_ShouldReturnCorrectStatistics()
    {
        // Arrange
        Athlete athlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        List<Activity> activities = new List<Activity>
    {
        new Activity
        {
            Id = 1,
            AthleteId = 1,
            Name = "Run 1",
            Distance = 5000,
            MovingTime = 1800, // 30 min
            ElapsedTime = 1900,
            TotalElevationGain = 100,
            SportType = "Run",
            StartDate = new DateTime(2024, 1, 15), // Januar
            StartDateLocal = new DateTime(2024, 1, 15),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        },
        new Activity
        {
            Id = 2,
            AthleteId = 1,
            Name = "Run 2",
            Distance = 8000,
            MovingTime = 2400, // 40 min
            ElapsedTime = 2500,
            TotalElevationGain = 150,
            SportType = "Run",
            StartDate = new DateTime(2024, 2, 10), // Februar
            StartDateLocal = new DateTime(2024, 2, 10),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        },
        new Activity
        {
            Id = 3,
            AthleteId = 1,
            Name = "Bike Ride",
            Distance = 20000,
            MovingTime = 3600, // 60 min
            ElapsedTime = 3700,
            TotalElevationGain = 300,
            SportType = "Ride",
            StartDate = new DateTime(2024, 1, 20), // Januar
            StartDateLocal = new DateTime(2024, 1, 20),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        },
    };

        await this.context.Athletes.AddAsync(athlete);
        await this.context.Activities.AddRangeAsync(activities);
        await this.context.SaveChangesAsync();

        // Act
        ActivityStatisticsDto result = await this.activityService.GetAthleteActivityStatisticsAsync(1, CancellationToken.None);

        // Assert
        Assert.NotNull(result);

        // Totals
        Assert.Equal(3, result.TotalActivities);
        Assert.Equal(33000, result.TotalDistance); // 5000 + 8000 + 20000
        Assert.Equal(TimeSpan.FromSeconds(7800), result.TotalDuration); // 1800 + 2400 + 3600
        Assert.Equal(550, result.TotalElevationGain); // 100 + 150 + 300

        // Sport type breakdown
        Assert.Equal(2, result.ActivitiesByType.Count);
        Assert.Equal(2, result.ActivitiesByType["Run"]);
        Assert.Equal(1, result.ActivitiesByType["Ride"]);

        // Month breakdown
        Assert.Equal(2, result.ActivitiesByMonth.Count);
        Assert.Equal(2, result.ActivitiesByMonth[1]); // Januar: 2 Activities
        Assert.Equal(1, result.ActivitiesByMonth[2]); // Februar: 1 Activity
    }

    [Fact]
    public async Task GetAthleteActivityStatisticsAsync_ShouldReturnZeroStatistics_WhenNoActivities()
    {
        // Arrange
        Athlete athlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        await this.context.Athletes.AddAsync(athlete);
        await this.context.SaveChangesAsync();

        // Act
        ActivityStatisticsDto result = await this.activityService.GetAthleteActivityStatisticsAsync(1, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalActivities);
        Assert.Equal(0, result.TotalDistance);
        Assert.Equal(TimeSpan.Zero, result.TotalDuration);
        Assert.Equal(0, result.TotalElevationGain);
        Assert.Empty(result.ActivitiesByType);
        Assert.Empty(result.ActivitiesByMonth);
    }
}
