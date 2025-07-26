namespace FitnessAnalyticsHub.Tests.Controllers;

using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Exceptions.Activities;
using FitnessAnalyticsHub.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class ActivityControllerTests
{
    private readonly Mock<IActivityService> mockActivityService;
    private readonly ActivityController controller;

    public ActivityControllerTests()
    {
        this.mockActivityService = new Mock<IActivityService>();
        this.controller = new ActivityController(this.mockActivityService.Object);
    }

    #region GetById Tests
    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithActivity()
    {
        // Arrange
        int activityId = 1;
        ActivityDto expectedActivity = new ActivityDto
        {
            Id = activityId,
            Name = "Test Activity",
            AthleteId = 1,
        };
        this.mockActivityService.Setup(s => s.GetActivityByIdAsync(activityId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedActivity);

        // Act
        ActionResult<ActivityDto> result = await this.controller.GetById(activityId, It.IsAny<CancellationToken>());

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        ActivityDto activity = Assert.IsType<ActivityDto>(okResult.Value);
        Assert.Equal(expectedActivity.Id, activity.Id);
        Assert.Equal(expectedActivity.Name, activity.Name);
    }

    [Fact]
    public Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        int invalidId = 999;

        this.mockActivityService
            .Setup(s => s.GetActivityByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ActivityNotFoundException(invalidId));

        // Act & Assert
        return Assert.ThrowsAsync<ActivityNotFoundException>(
            () => this.controller.GetById(invalidId, It.IsAny<CancellationToken>()));
    }
    #endregion

    #region GetByAthleteId Tests
    [Fact]
    public async Task GetByAthleteId_WithValidAthleteId_ReturnsOkWithActivities()
    {
        // Arrange
        int athleteId = 1;
        List<ActivityDto> expectedActivities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, Name = "Activity 1", AthleteId = athleteId },
            new ActivityDto { Id = 2, Name = "Activity 2", AthleteId = athleteId },
        };
        this.mockActivityService.Setup(s => s.GetActivitiesByAthleteIdAsync(athleteId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedActivities);

        // Act
        ActionResult<IEnumerable<ActivityDto>> result = await this.controller.GetByAthleteId(athleteId, It.IsAny<CancellationToken>());

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IEnumerable<ActivityDto> activities = Assert.IsAssignableFrom<IEnumerable<ActivityDto>>(okResult.Value);
        Assert.Equal(expectedActivities.Count, activities.Count());
    }

    [Fact]
    public async Task GetByAthleteId_WithEmptyResult_ReturnsOkWithEmptyList()
    {
        // Arrange
        int athleteId = 1;
        List<ActivityDto> expectedActivities = new List<ActivityDto>();
        this.mockActivityService.Setup(s => s.GetActivitiesByAthleteIdAsync(athleteId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedActivities);

        // Act
        ActionResult<IEnumerable<ActivityDto>> result = await this.controller.GetByAthleteId(athleteId, It.IsAny<CancellationToken>());

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IEnumerable<ActivityDto> activities = Assert.IsAssignableFrom<IEnumerable<ActivityDto>>(okResult.Value);
        Assert.Empty(activities);
    }
    #endregion

    #region Create Tests
    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        CreateActivityDto createDto = new CreateActivityDto
        {
            Name = "New Activity",
            AthleteId = 1,
        };
        ActivityDto createdActivity = new ActivityDto
        {
            Id = 1,
            Name = createDto.Name,
            AthleteId = createDto.AthleteId,
        };
        this.mockActivityService.Setup(s => s.CreateActivityAsync(createDto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(createdActivity);

        // Act
        ActionResult<ActivityDto> result = await this.controller.Create(createDto, It.IsAny<CancellationToken>());

        // Assert
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ActivityController.GetById), createdResult.ActionName);
        Assert.Equal(createdActivity.Id, createdResult.RouteValues["id"]);
        ActivityDto activity = Assert.IsType<ActivityDto>(createdResult.Value);
        Assert.Equal(createdActivity.Id, activity.Id);
    }
    #endregion

    #region Update Tests
    [Fact]
    public async Task Update_WithMatchingIds_ReturnsNoContent()
    {
        // Arrange
        int id = 1;
        UpdateActivityDto updateDto = new UpdateActivityDto
        {
            Id = id,
            Name = "Updated Activity",
        };
        this.mockActivityService.Setup(s => s.UpdateActivityAsync(updateDto, It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await this.controller.Update(id, updateDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.mockActivityService.Verify(s => s.UpdateActivityAsync(updateDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        int id = 1;
        UpdateActivityDto updateDto = new UpdateActivityDto
        {
            Id = 2,
            Name = "Updated Activity",
        };

        // Act
        IActionResult result = await this.controller.Update(id, updateDto, It.IsAny<CancellationToken>());

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("ID in der URL stimmt nicht mit der ID im Körper überein.", badRequestResult.Value);
        this.mockActivityService.Verify(s => s.UpdateActivityAsync(It.IsAny<UpdateActivityDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion

    #region Delete Tests
    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        int id = 1;
        this.mockActivityService.Setup(s => s.DeleteActivityAsync(id, It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await this.controller.Delete(id, It.IsAny<CancellationToken>());

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.mockActivityService.Verify(s => s.DeleteActivityAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region ImportFromStrava Tests
    [Fact]
    public async Task ImportFromStrava_WithValidParameters_ReturnsOkWithActivities()
    {
        // Arrange
        List<ActivityDto> importedActivities = new List<ActivityDto>
            {
                new ActivityDto { Id = 1, Name = "Strava Activity 1", AthleteId = 1 },
                new ActivityDto { Id = 2, Name = "Strava Activity 2", AthleteId = 1 },
            };

        this.mockActivityService.Setup(s => s.ImportActivitiesFromStravaAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(importedActivities);

        // Act
        ActionResult<IEnumerable<ActivityDto>> result = await this.controller.ImportFromStrava(It.IsAny<CancellationToken>());

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IEnumerable<ActivityDto> activities = Assert.IsAssignableFrom<IEnumerable<ActivityDto>>(okResult.Value);
        Assert.Equal(importedActivities.Count, activities.Count());
    }
    #endregion

    #region GetStatistics Tests
    [Fact]
    public async Task GetStatistics_WithValidAthleteId_ReturnsOkWithStatistics()
    {
        // Arrange
        int athleteId = 1;
        ActivityStatisticsDto expectedStatistics = new ActivityStatisticsDto
        {
            TotalActivities = 10,
            TotalDistance = 100.5,
            TotalDuration = TimeSpan.FromHours(25),
            TotalElevationGain = 2500.75,
            ActivitiesByType = new Dictionary<string, int>
            {
                { "Running", 5 },
                { "Cycling", 3 },
                { "Swimming", 2 },
            },
            ActivitiesByMonth = new Dictionary<int, int>
            {
                { 1, 3 },
                { 2, 4 },
                { 3, 3 },
            },
        };
        this.mockActivityService.Setup(s => s.GetAthleteActivityStatisticsAsync(athleteId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedStatistics);

        // Act
        ActionResult<ActivityStatisticsDto> result = await this.controller.GetStatistics(athleteId, It.IsAny<CancellationToken>());

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        ActivityStatisticsDto statistics = Assert.IsType<ActivityStatisticsDto>(okResult.Value);
        Assert.Equal(expectedStatistics.TotalActivities, statistics.TotalActivities);
        Assert.Equal(expectedStatistics.TotalDistance, statistics.TotalDistance);
        Assert.Equal(expectedStatistics.TotalDuration, statistics.TotalDuration);
        Assert.Equal(expectedStatistics.TotalElevationGain, statistics.TotalElevationGain);
        Assert.Equal(expectedStatistics.ActivitiesByType.Count, statistics.ActivitiesByType.Count);
        Assert.Equal(expectedStatistics.ActivitiesByMonth.Count, statistics.ActivitiesByMonth.Count);
    }
    #endregion
}
