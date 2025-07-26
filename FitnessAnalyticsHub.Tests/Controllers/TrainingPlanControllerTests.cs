namespace FitnessAnalyticsHub.Tests.Controllers;

using FitnessAnalyticsHub.Application;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Tests.Base;
using FitnessAnalyticsHub.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class TrainingPlanControllerTests : ControllerTestBase<TrainingPlanController>
{
    private readonly Mock<ITrainingPlanService> mockTrainingPlanService;
    private readonly TrainingPlanController controller;

    public TrainingPlanControllerTests()
    {
        this.mockTrainingPlanService = new Mock<ITrainingPlanService>();
        this.controller = new TrainingPlanController(this.mockTrainingPlanService.Object);
    }

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithTrainingPlan()
    {
        // Arrange
        int trainingPlanId = 1;
        TrainingPlanDto expectedTrainingPlan = new TrainingPlanDto
        {
            Id = trainingPlanId,
            Name = "Marathon Training",
            AthleteId = 1,
            AthleteName = "Test Athlete",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(90),
            Goal = Domain.Enums.TrainingGoal.EnduranceImprovement,
        };

        this.mockTrainingPlanService
            .Setup(s => s.GetTrainingPlanByIdAsync(trainingPlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTrainingPlan);

        // Act
        ActionResult<TrainingPlanDto> result = await this.controller.GetById(trainingPlanId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        TrainingPlanDto trainingPlan = Assert.IsType<TrainingPlanDto>(okResult.Value);
        Assert.Equal(expectedTrainingPlan.Id, trainingPlan.Id);
        Assert.Equal(expectedTrainingPlan.Name, trainingPlan.Name);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        int invalidId = 999;
        this.mockTrainingPlanService
            .Setup(s => s.GetTrainingPlanByIdAsync(invalidId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrainingPlanDto?)null);

        // Act
        ActionResult<TrainingPlanDto> result = await this.controller.GetById(invalidId, CancellationToken.None);

        // Assert
        NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains("999", notFoundResult.Value?.ToString());
    }

    #endregion

    #region GetByAthleteId Tests

    [Fact]
    public async Task GetByAthleteId_WithValidAthleteId_ReturnsOkWithTrainingPlans()
    {
        // Arrange
        int athleteId = 1;
        List<TrainingPlanDto> expectedTrainingPlans = new List<TrainingPlanDto>
        {
            new TrainingPlanDto
            {
                Id = 1,
                Name = "Marathon Training",
                AthleteId = athleteId,
                AthleteName = "Test Athlete",
            },
            new TrainingPlanDto
            {
                Id = 2,
                Name = "5K Training",
                AthleteId = athleteId,
                AthleteName = "Test Athlete",
            },
        };

        this.mockTrainingPlanService
            .Setup(s => s.GetTrainingPlansByAthleteIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTrainingPlans);

        // Act
        ActionResult<IEnumerable<TrainingPlanDto>> result = await this.controller.GetByAthleteId(athleteId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        IEnumerable<TrainingPlanDto> trainingPlans = Assert.IsAssignableFrom<IEnumerable<TrainingPlanDto>>(okResult.Value);
        Assert.Equal(expectedTrainingPlans.Count, trainingPlans.Count());
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        CreateTrainingPlanDto createDto = new CreateTrainingPlanDto
        {
            AthleteId = 1,
            Name = "New Training Plan",
            Description = "Test Description",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(60),
            Goal = Domain.Enums.TrainingGoal.GeneralFitness,
        };

        TrainingPlanDto createdTrainingPlan = new TrainingPlanDto
        {
            Id = 1,
            AthleteId = createDto.AthleteId,
            Name = createDto.Name,
            Description = createDto.Description,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            Goal = createDto.Goal,
            AthleteName = "Test Athlete",
        };

        this.mockTrainingPlanService
            .Setup(s => s.CreateTrainingPlanAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTrainingPlan);

        // Act
        ActionResult<TrainingPlanDto> result = await this.controller.Create(createDto, CancellationToken.None);

        // Assert
        CreatedAtActionResult createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TrainingPlanController.GetById), createdResult.ActionName);
        Assert.Equal(createdTrainingPlan.Id, createdResult.RouteValues!["id"]);

        TrainingPlanDto trainingPlan = Assert.IsType<TrainingPlanDto>(createdResult.Value);
        Assert.Equal(createdTrainingPlan.Id, trainingPlan.Id);
        Assert.Equal(createdTrainingPlan.Name, trainingPlan.Name);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithMatchingIds_ReturnsNoContent()
    {
        // Arrange
        int id = 1;
        UpdateTrainingPlanDto updateDto = new UpdateTrainingPlanDto
        {
            Id = id,
            Name = "Updated Training Plan",
            Description = "Updated Description",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(90),
            Goal = Domain.Enums.TrainingGoal.RacePreparation,
        };

        this.mockTrainingPlanService
            .Setup(s => s.UpdateTrainingPlanAsync(updateDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await this.controller.Update(id, updateDto, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.mockTrainingPlanService.Verify(s => s.UpdateTrainingPlanAsync(updateDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        int id = 1;
        UpdateTrainingPlanDto updateDto = new UpdateTrainingPlanDto
        {
            Id = 2, // Different ID
            Name = "Updated Training Plan",
        };

        // Act
        IActionResult result = await this.controller.Update(id, updateDto, CancellationToken.None);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("ID in der URL stimmt nicht mit der ID im Körper überein.", badRequestResult.Value);

        // Service should never be called
        this.mockTrainingPlanService.Verify(s => s.UpdateTrainingPlanAsync(It.IsAny<UpdateTrainingPlanDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithServiceException_ReturnsNotFound()
    {
        // Arrange
        int id = 1;
        UpdateTrainingPlanDto updateDto = new UpdateTrainingPlanDto
        {
            Id = id,
            Name = "Updated Training Plan",
        };

        this.mockTrainingPlanService
            .Setup(s => s.UpdateTrainingPlanAsync(updateDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Training plan not found"));

        // Act
        IActionResult result = await this.controller.Update(id, updateDto, CancellationToken.None);

        // Assert
        NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Training plan not found", notFoundResult.Value);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        int id = 1;
        this.mockTrainingPlanService
            .Setup(s => s.DeleteTrainingPlanAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await this.controller.Delete(id, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.mockTrainingPlanService.Verify(s => s.DeleteTrainingPlanAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_WithServiceException_ReturnsNotFound()
    {
        // Arrange
        int id = 999;
        this.mockTrainingPlanService
            .Setup(s => s.DeleteTrainingPlanAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Training plan not found"));

        // Act
        IActionResult result = await this.controller.Delete(id, CancellationToken.None);

        // Assert
        NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Training plan not found", notFoundResult.Value);
    }

    #endregion

    #region Planned Activities Tests

    [Fact]
    public async Task AddPlannedActivity_WithValidData_ReturnsOkWithPlannedActivity()
    {
        // Arrange
        int trainingPlanId = 1;
        CreatePlannedActivityDto createDto = new CreatePlannedActivityDto
        {
            TrainingPlanId = trainingPlanId,
            Title = "Morning Run",
            Description = "Easy 5K run",
            SportType = "Run",
            PlannedDate = DateTime.Now.AddDays(1),
            PlannedDurationMinutes = 30,
            PlannedDistance = 5.0,
        };

        PlannedActivityDto createdPlannedActivity = new PlannedActivityDto
        {
            Id = 1,
            TrainingPlanId = trainingPlanId,
            Title = createDto.Title,
            Description = createDto.Description,
            SportType = createDto.SportType,
            PlannedDate = createDto.PlannedDate,
            PlannedDuration = TimeSpan.FromMinutes(createDto.PlannedDurationMinutes ?? 0),
            PlannedDistance = createDto.PlannedDistance,
        };

        this.mockTrainingPlanService
            .Setup(s => s.AddPlannedActivityAsync(trainingPlanId, createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPlannedActivity);

        // Act
        ActionResult<PlannedActivityDto> result = await this.controller.AddPlannedActivity(trainingPlanId, createDto, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        PlannedActivityDto plannedActivity = Assert.IsType<PlannedActivityDto>(okResult.Value);
        Assert.Equal(createdPlannedActivity.Id, plannedActivity.Id);
        Assert.Equal(createdPlannedActivity.Title, plannedActivity.Title);
    }

    [Fact]
    public async Task AddPlannedActivity_WithMismatchedTrainingPlanIds_ReturnsBadRequest()
    {
        // Arrange
        int trainingPlanId = 1;
        CreatePlannedActivityDto createDto = new CreatePlannedActivityDto
        {
            TrainingPlanId = 2, // Different ID
            Title = "Morning Run",
        };

        // Act
        ActionResult<PlannedActivityDto> result = await this.controller.AddPlannedActivity(trainingPlanId, createDto, CancellationToken.None);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("TrainingPlanID in der URL stimmt nicht mit der ID im Körper überein.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdatePlannedActivity_WithMatchingIds_ReturnsNoContent()
    {
        // Arrange
        int plannedActivityId = 1;
        UpdatePlannedActivityDto updateDto = new UpdatePlannedActivityDto
        {
            Id = plannedActivityId,
            Title = "Updated Activity",
            SportType = "Run",
            PlannedDate = DateTime.Now.AddDays(2),
        };

        this.mockTrainingPlanService
            .Setup(s => s.UpdatePlannedActivityAsync(updateDto, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        IActionResult result = await this.controller.UpdatePlannedActivity(plannedActivityId, updateDto, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.mockTrainingPlanService.Verify(s => s.UpdatePlannedActivityAsync(updateDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkPlannedActivityAsCompleted_WithValidData_ReturnsOkWithUpdatedActivity()
    {
        // Arrange
        int plannedActivityId = 1;
        int activityId = 5;
        PlannedActivityDto completedActivity = new PlannedActivityDto
        {
            Id = plannedActivityId,
            Title = "Morning Run",
            CompletedActivity = new ActivityDto { Id = activityId, Name = "Completed Run" },
        };

        this.mockTrainingPlanService
            .Setup(s => s.MarkPlannedActivityAsCompletedAsync(plannedActivityId, activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedActivity);

        // Act
        ActionResult<PlannedActivityDto> result = await this.controller.MarkPlannedActivityAsCompleted(plannedActivityId, activityId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        PlannedActivityDto plannedActivity = Assert.IsType<PlannedActivityDto>(okResult.Value);
        Assert.True(plannedActivity.IsCompleted);
        Assert.NotNull(plannedActivity.CompletedActivity);
        Assert.Equal(activityId, plannedActivity.CompletedActivity.Id);
    }

    #endregion
}