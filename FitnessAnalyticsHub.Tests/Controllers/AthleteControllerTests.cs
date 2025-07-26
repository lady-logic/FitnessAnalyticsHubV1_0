using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FitnessAnalyticsHub.Tests.Controllers;

public class AthleteControllerTests
{
    private readonly Mock<IAthleteService> mockAthleteService;
    private readonly AthleteController controller;

    public AthleteControllerTests()
    {
        this.mockAthleteService = new Mock<IAthleteService>();
        this.controller = new AthleteController(this.mockAthleteService.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithExistingAthletes_ReturnsOkWithAthletes()
    {
        // Arrange
        var expectedAthletes = new List<AthleteDto>
        {
            new AthleteDto { Id = 1, FirstName = "Max", LastName = "Mustermann", Email = "max@example.com" },
            new AthleteDto { Id = 2, FirstName = "Anna", LastName = "Schmidt", Email = "anna@example.com" },
        };
        this.mockAthleteService.Setup(s => s.GetAllAthletesAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedAthletes);

        // Act
        var result = await this.controller.GetAll(It.IsAny<CancellationToken>());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var athletes = Assert.IsAssignableFrom<IEnumerable<AthleteDto>>(okResult.Value);
        Assert.Equal(expectedAthletes.Count, athletes.Count());
    }

    [Fact]
    public async Task GetAll_WithNoAthletes_ReturnsOkWithEmptyList()
    {
        // Arrange
        var expectedAthletes = new List<AthleteDto>();
        this.mockAthleteService.Setup(s => s.GetAllAthletesAsync(It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedAthletes);

        // Act
        var result = await this.controller.GetAll(It.IsAny<CancellationToken>());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var athletes = Assert.IsAssignableFrom<IEnumerable<AthleteDto>>(okResult.Value);
        Assert.Empty(athletes);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithAthlete()
    {
        // Arrange
        var athleteId = 1;
        var expectedAthlete = new AthleteDto
        {
            Id = athleteId,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@example.com",
        };
        this.mockAthleteService.Setup(s => s.GetAthleteByIdAsync(athleteId, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(expectedAthlete);

        // Act
        var result = await this.controller.GetById(athleteId, It.IsAny<CancellationToken>());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var athlete = Assert.IsType<AthleteDto>(okResult.Value);
        Assert.Equal(expectedAthlete.Id, athlete.Id);
        Assert.Equal(expectedAthlete.FirstName, athlete.FirstName);
        Assert.Equal(expectedAthlete.LastName, athlete.LastName);
    }
    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateAthleteDto
        {
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@example.com",
        };
        var createdAthlete = new AthleteDto
        {
            Id = 1,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email,
        };
        this.mockAthleteService.Setup(s => s.CreateAthleteAsync(createDto, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(createdAthlete);

        // Act
        var result = await this.controller.Create(createDto, It.IsAny<CancellationToken>());

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(AthleteController.GetById), createdResult.ActionName);
        Assert.Equal(createdAthlete.Id, createdResult.RouteValues["id"]);
        var athlete = Assert.IsType<AthleteDto>(createdResult.Value);
        Assert.Equal(createdAthlete.Id, athlete.Id);
        Assert.Equal(createdAthlete.FirstName, athlete.FirstName);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithMatchingIds_ReturnsNoContent()
    {
        // Arrange
        var id = 1;
        var updateDto = new UpdateAthleteDto
        {
            Id = id,
            FirstName = "Max Updated",
            LastName = "Mustermann",
            Email = "max.updated@example.com",
        };
        this.mockAthleteService.Setup(s => s.UpdateAthleteAsync(updateDto, It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

        // Act
        var result = await this.controller.Update(id, updateDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.mockAthleteService.Verify(s => s.UpdateAthleteAsync(updateDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var updateDto = new UpdateAthleteDto
        {
            Id = 2,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@example.com",
        };

        // Act
        var result = await this.controller.Update(id, updateDto, It.IsAny<CancellationToken>());

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("ID in der URL stimmt nicht mit der ID im Körper überein.", badRequestResult.Value);
        this.mockAthleteService.Verify(s => s.UpdateAthleteAsync(It.IsAny<UpdateAthleteDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var id = 1;
        this.mockAthleteService.Setup(s => s.DeleteAthleteAsync(id, It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

        // Act
        var result = await this.controller.Delete(id, It.IsAny<CancellationToken>());

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.mockAthleteService.Verify(s => s.DeleteAthleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region ImportFromStrava Tests

    [Fact]
    public async Task ImportFromStrava_WithValidToken_ReturnsOkWithAthlete()
    {
        // Arrange
        var accessToken = "valid_strava_token";
        var importedAthlete = new AthleteDto
        {
            Id = 1,
            FirstName = "Strava",
            LastName = "User",
            Email = "strava.user@example.com",
        };
        this.mockAthleteService.Setup(s => s.ImportAthleteFromStravaAsync(accessToken, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(importedAthlete);

        // Act
        var result = await this.controller.ImportFromStrava(accessToken, It.IsAny<CancellationToken>());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var athlete = Assert.IsType<AthleteDto>(okResult.Value);
        Assert.Equal(importedAthlete.Id, athlete.Id);
        Assert.Equal(importedAthlete.FirstName, athlete.FirstName);
    }
    #endregion
}
