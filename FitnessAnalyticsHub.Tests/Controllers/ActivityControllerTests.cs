using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Exceptions.Activities;
using FitnessAnalyticsHub.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FitnessAnalyticsHub.Tests.Controllers;

public class ActivityControllerTests
{
    private readonly Mock<IActivityService> _mockActivityService;
    private readonly ActivityController _controller;

    public ActivityControllerTests()
    {
        _mockActivityService = new Mock<IActivityService>();
        _controller = new ActivityController(_mockActivityService.Object);
    }

    #region GetById Tests
    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithActivity()
    {
        // Arrange
        var activityId = 1;
        var expectedActivity = new ActivityDto
        {
            Id = activityId,
            Name = "Test Activity",
            AthleteId = 1
        };
        _mockActivityService.Setup(s => s.GetActivityByIdAsync(activityId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedActivity);

        // Act
        var result = await _controller.GetById(activityId, It.IsAny<CancellationToken>());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var activity = Assert.IsType<ActivityDto>(okResult.Value);
        Assert.Equal(expectedActivity.Id, activity.Id);
        Assert.Equal(expectedActivity.Name, activity.Name);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = 999;

        _mockActivityService
            .Setup(s => s.GetActivityByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ActivityNotFoundException(invalidId));

        // Act & Assert
        await Assert.ThrowsAsync<ActivityNotFoundException>(
            () => _controller.GetById(invalidId, It.IsAny<CancellationToken>())
        );
    }
    #endregion

    #region GetByAthleteId Tests
    [Fact]
    public async Task GetByAthleteId_WithValidAthleteId_ReturnsOkWithActivities()
    {
        // Arrange
        var athleteId = 1;
        var expectedActivities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, Name = "Activity 1", AthleteId = athleteId },
            new ActivityDto { Id = 2, Name = "Activity 2", AthleteId = athleteId }
        };
        _mockActivityService.Setup(s => s.GetActivitiesByAthleteIdAsync(athleteId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedActivities);

        // Act
        var result = await _controller.GetByAthleteId(athleteId, It.IsAny<CancellationToken>());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var activities = Assert.IsAssignableFrom<IEnumerable<ActivityDto>>(okResult.Value);
        Assert.Equal(expectedActivities.Count, activities.Count());
    }

    [Fact]
    public async Task GetByAthleteId_WithEmptyResult_ReturnsOkWithEmptyList()
    {
        // Arrange
        var athleteId = 1;
        var expectedActivities = new List<ActivityDto>();
        _mockActivityService.Setup(s => s.GetActivitiesByAthleteIdAsync(athleteId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedActivities);

        // Act
        var result = await _controller.GetByAthleteId(athleteId, It.IsAny<CancellationToken>());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var activities = Assert.IsAssignableFrom<IEnumerable<ActivityDto>>(okResult.Value);
        Assert.Empty(activities);
    }
    #endregion

    #region Create Tests
    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateActivityDto
        {
            Name = "New Activity",
            AthleteId = 1
        };
        var createdActivity = new ActivityDto
        {
            Id = 1,
            Name = createDto.Name,
            AthleteId = createDto.AthleteId
        };
        _mockActivityService.Setup(s => s.CreateActivityAsync(createDto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(createdActivity);

        // Act
        var result = await _controller.Create(createDto, It.IsAny<CancellationToken>());

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ActivityController.GetById), createdResult.ActionName);
        Assert.Equal(createdActivity.Id, createdResult.RouteValues["id"]);
        var activity = Assert.IsType<ActivityDto>(createdResult.Value);
        Assert.Equal(createdActivity.Id, activity.Id);
    }
    #endregion

    #region Update Tests
    [Fact]
    public async Task Update_WithMatchingIds_ReturnsNoContent()
    {
        // Arrange
        var id = 1;
        var updateDto = new UpdateActivityDto
        {
            Id = id,
            Name = "Updated Activity"
        };
        _mockActivityService.Setup(s => s.UpdateActivityAsync(updateDto, It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(id, updateDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockActivityService.Verify(s => s.UpdateActivityAsync(updateDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var updateDto = new UpdateActivityDto
        {
            Id = 2,
            Name = "Updated Activity"
        };

        // Act
        var result = await _controller.Update(id, updateDto, It.IsAny<CancellationToken>());

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("ID in der URL stimmt nicht mit der ID im Körper überein.", badRequestResult.Value);
        _mockActivityService.Verify(s => s.UpdateActivityAsync(It.IsAny<UpdateActivityDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion

    #region Delete Tests
    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var id = 1;
        _mockActivityService.Setup(s => s.DeleteActivityAsync(id, It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(id, It.IsAny<CancellationToken>());

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockActivityService.Verify(s => s.DeleteActivityAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region ImportFromStrava Tests
    [Fact]
    public async Task ImportFromStrava_WithValidParameters_ReturnsOkWithActivities()
    {
        // Arrange
        var importedActivities = new List<ActivityDto>
            {
                new ActivityDto { Id = 1, Name = "Strava Activity 1", AthleteId = 1 },
                new ActivityDto { Id = 2, Name = "Strava Activity 2", AthleteId = 1 }
            };

        _mockActivityService.Setup(s => s.ImportActivitiesFromStravaAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(importedActivities);

        // Act
        var result = await _controller.ImportFromStrava(It.IsAny<CancellationToken>());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var activities = Assert.IsAssignableFrom<IEnumerable<ActivityDto>>(okResult.Value);
        Assert.Equal(importedActivities.Count, activities.Count());
    }
    #endregion

    #region GetStatistics Tests
    [Fact]
    public async Task GetStatistics_WithValidAthleteId_ReturnsOkWithStatistics()
    {
        // Arrange
        var athleteId = 1;
        var expectedStatistics = new ActivityStatisticsDto
        {
            TotalActivities = 10,
            TotalDistance = 100.5,
            TotalDuration = TimeSpan.FromHours(25),
            TotalElevationGain = 2500.75,
            ActivitiesByType = new Dictionary<string, int>
            {
                { "Running", 5 },
                { "Cycling", 3 },
                { "Swimming", 2 }
            },
            ActivitiesByMonth = new Dictionary<int, int>
            {
                { 1, 3 },
                { 2, 4 },
                { 3, 3 }
            }
        };
        _mockActivityService.Setup(s => s.GetAthleteActivityStatisticsAsync(athleteId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedStatistics);

        // Act
        var result = await _controller.GetStatistics(athleteId, It.IsAny<CancellationToken>());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var statistics = Assert.IsType<ActivityStatisticsDto>(okResult.Value);
        Assert.Equal(expectedStatistics.TotalActivities, statistics.TotalActivities);
        Assert.Equal(expectedStatistics.TotalDistance, statistics.TotalDistance);
        Assert.Equal(expectedStatistics.TotalDuration, statistics.TotalDuration);
        Assert.Equal(expectedStatistics.TotalElevationGain, statistics.TotalElevationGain);
        Assert.Equal(expectedStatistics.ActivitiesByType.Count, statistics.ActivitiesByType.Count);
        Assert.Equal(expectedStatistics.ActivitiesByMonth.Count, statistics.ActivitiesByMonth.Count);
    }
    #endregion
}
