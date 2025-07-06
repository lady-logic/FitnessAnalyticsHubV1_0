using FitnessAnalyticsHub.Application;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Tests.Base;
using FitnessAnalyticsHub.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FitnessAnalyticsHub.Tests.Controllers;

public class TrainingPlanControllerTests : ControllerTestBase<TrainingPlanController>
{
    private readonly Mock<ITrainingPlanService> _mockTrainingPlanService;
    private readonly TrainingPlanController _controller;

    public TrainingPlanControllerTests()
    {
        _mockTrainingPlanService = new Mock<ITrainingPlanService>();
        _controller = new TrainingPlanController(_mockTrainingPlanService.Object);
    }

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithTrainingPlan()
    {
        // Arrange
        var trainingPlanId = 1;
        var expectedTrainingPlan = new TrainingPlanDto
        {
            Id = trainingPlanId,
            Name = "Marathon Training",
            AthleteId = 1,
            AthleteName = "Test Athlete",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(90),
            Goal = Domain.Enums.TrainingGoal.EnduranceImprovement
        };

        _mockTrainingPlanService
            .Setup(s => s.GetTrainingPlanByIdAsync(trainingPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTrainingPlan);

        // Act
        var result = await _controller.GetById(trainingPlanId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var trainingPlan = Assert.IsType<TrainingPlanDto>(okResult.Value);
        Assert.Equal(expectedTrainingPlan.Id, trainingPlan.Id);
        Assert.Equal(expectedTrainingPlan.Name, trainingPlan.Name);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = 999;
        _mockTrainingPlanService
            .Setup(s => s.GetTrainingPlanByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrainingPlanDto?)null);

        // Act
        var result = await _controller.GetById(invalidId, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains("999", notFoundResult.Value?.ToString());
    }

    #endregion

    #region GetByAthleteId Tests

    [Fact]
    public async Task GetByAthleteId_WithValidAthleteId_ReturnsOkWithTrainingPlans()
    {
        // Arrange
        var athleteId = 1;
        var expectedTrainingPlans = new List<TrainingPlanDto>
        {
            new TrainingPlanDto
            {
                Id = 1,
                Name = "Marathon Training",
                AthleteId = athleteId,
                AthleteName = "Test Athlete"
            },
            new TrainingPlanDto
            {
                Id = 2,
                Name = "5K Training",
                AthleteId = athleteId,
                AthleteName = "Test Athlete"
            }
        };

        _mockTrainingPlanService
            .Setup(s => s.GetTrainingPlansByAthleteIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTrainingPlans);

        // Act
        var result = await _controller.GetByAthleteId(athleteId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var trainingPlans = Assert.IsAssignableFrom<IEnumerable<TrainingPlanDto>>(okResult.Value);
        Assert.Equal(expectedTrainingPlans.Count, trainingPlans.Count());
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateTrainingPlanDto
        {
            AthleteId = 1,
            Name = "New Training Plan",
            Description = "Test Description",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(60),
            Goal = Domain.Enums.TrainingGoal.GeneralFitness
        };

        var createdTrainingPlan = new TrainingPlanDto
        {
            Id = 1,
            AthleteId = createDto.AthleteId,
            Name = createDto.Name,
            Description = createDto.Description,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            Goal = createDto.Goal,
            AthleteName = "Test Athlete"
        };

        _mockTrainingPlanService
            .Setup(s => s.CreateTrainingPlanAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTrainingPlan);

        // Act
        var result = await _controller.Create(createDto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TrainingPlanController.GetById), createdResult.ActionName);
        Assert.Equal(createdTrainingPlan.Id, createdResult.RouteValues!["id"]);

        var trainingPlan = Assert.IsType<TrainingPlanDto>(createdResult.Value);
        Assert.Equal(createdTrainingPlan.Id, trainingPlan.Id);
        Assert.Equal(createdTrainingPlan.Name, trainingPlan.Name);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithMatchingIds_ReturnsNoContent()
    {
        // Arrange
        var id = 1;
        var updateDto = new UpdateTrainingPlanDto
        {
            Id = id,
            Name = "Updated Training Plan",
            Description = "Updated Description",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(90),
            Goal = Domain.Enums.TrainingGoal.RacePreparation
        };

        _mockTrainingPlanService
            .Setup(s => s.UpdateTrainingPlanAsync(updateDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(id, updateDto, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockTrainingPlanService.Verify(s => s.UpdateTrainingPlanAsync(updateDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var updateDto = new UpdateTrainingPlanDto
        {
            Id = 2, // Different ID
            Name = "Updated Training Plan"
        };

        // Act
        var result = await _controller.Update(id, updateDto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("ID in der URL stimmt nicht mit der ID im Körper überein.", badRequestResult.Value);

        // Service should never be called
        _mockTrainingPlanService.Verify(s => s.UpdateTrainingPlanAsync(It.IsAny<UpdateTrainingPlanDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithServiceException_ReturnsNotFound()
    {
        // Arrange
        var id = 1;
        var updateDto = new UpdateTrainingPlanDto
        {
            Id = id,
            Name = "Updated Training Plan"
        };

        _mockTrainingPlanService
            .Setup(s => s.UpdateTrainingPlanAsync(updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Training plan not found"));

        // Act
        var result = await _controller.Update(id, updateDto, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Training plan not found", notFoundResult.Value);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var id = 1;
        _mockTrainingPlanService
            .Setup(s => s.DeleteTrainingPlanAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockTrainingPlanService.Verify(s => s.DeleteTrainingPlanAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithServiceException_ReturnsNotFound()
    {
        // Arrange
        var id = 999;
        _mockTrainingPlanService
            .Setup(s => s.DeleteTrainingPlanAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Training plan not found"));

        // Act
        var result = await _controller.Delete(id, CancellationToken.None);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Training plan not found", notFoundResult.Value);
    }

    #endregion

    #region Planned Activities Tests

    [Fact]
    public async Task AddPlannedActivity_WithValidData_ReturnsOkWithPlannedActivity()
    {
        // Arrange
        var trainingPlanId = 1;
        var createDto = new CreatePlannedActivityDto
        {
            TrainingPlanId = trainingPlanId,
            Title = "Morning Run",
            Description = "Easy 5K run",
            SportType = "Run",
            PlannedDate = DateTime.Now.AddDays(1),
            PlannedDurationMinutes = 30,
            PlannedDistance = 5.0
        };

        var createdPlannedActivity = new PlannedActivityDto
        {
            Id = 1,
            TrainingPlanId = trainingPlanId,
            Title = createDto.Title,
            Description = createDto.Description,
            SportType = createDto.SportType,
            PlannedDate = createDto.PlannedDate,
            PlannedDuration = TimeSpan.FromMinutes(createDto.PlannedDurationMinutes ?? 0),
            PlannedDistance = createDto.PlannedDistance
        };

        _mockTrainingPlanService
            .Setup(s => s.AddPlannedActivityAsync(trainingPlanId, createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPlannedActivity);

        // Act
        var result = await _controller.AddPlannedActivity(trainingPlanId, createDto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var plannedActivity = Assert.IsType<PlannedActivityDto>(okResult.Value);
        Assert.Equal(createdPlannedActivity.Id, plannedActivity.Id);
        Assert.Equal(createdPlannedActivity.Title, plannedActivity.Title);
    }

    [Fact]
    public async Task AddPlannedActivity_WithMismatchedTrainingPlanIds_ReturnsBadRequest()
    {
        // Arrange
        var trainingPlanId = 1;
        var createDto = new CreatePlannedActivityDto
        {
            TrainingPlanId = 2, // Different ID
            Title = "Morning Run"
        };

        // Act
        var result = await _controller.AddPlannedActivity(trainingPlanId, createDto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("TrainingPlanID in der URL stimmt nicht mit der ID im Körper überein.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdatePlannedActivity_WithMatchingIds_ReturnsNoContent()
    {
        // Arrange
        var plannedActivityId = 1;
        var updateDto = new UpdatePlannedActivityDto
        {
            Id = plannedActivityId,
            Title = "Updated Activity",
            SportType = "Run",
            PlannedDate = DateTime.Now.AddDays(2)
        };

        _mockTrainingPlanService
            .Setup(s => s.UpdatePlannedActivityAsync(updateDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdatePlannedActivity(plannedActivityId, updateDto, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockTrainingPlanService.Verify(s => s.UpdatePlannedActivityAsync(updateDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkPlannedActivityAsCompleted_WithValidData_ReturnsOkWithUpdatedActivity()
    {
        // Arrange
        var plannedActivityId = 1;
        var activityId = 5;
        var completedActivity = new PlannedActivityDto
        {
            Id = plannedActivityId,
            Title = "Morning Run",
            CompletedActivity = new ActivityDto { Id = activityId, Name = "Completed Run" },
        };

        _mockTrainingPlanService
            .Setup(s => s.MarkPlannedActivityAsCompletedAsync(plannedActivityId, activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedActivity);

        // Act
        var result = await _controller.MarkPlannedActivityAsCompleted(plannedActivityId, activityId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var plannedActivity = Assert.IsType<PlannedActivityDto>(okResult.Value);
        Assert.True(plannedActivity.IsCompleted);
        Assert.NotNull(plannedActivity.CompletedActivity);
        Assert.Equal(activityId, plannedActivity.CompletedActivity.Id);
    }

    #endregion
}