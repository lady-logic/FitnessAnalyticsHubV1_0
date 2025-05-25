using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FitnessAnalyticsHub.Tests.Controllers
{
    public class AthleteControllerTests
    {
        private readonly Mock<IAthleteService> _mockAthleteService;
        private readonly AthleteController _controller;

        public AthleteControllerTests()
        {
            _mockAthleteService = new Mock<IAthleteService>();
            _controller = new AthleteController(_mockAthleteService.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_WithExistingAthletes_ReturnsOkWithAthletes()
        {
            // Arrange
            var expectedAthletes = new List<AthleteDto>
            {
                new AthleteDto { Id = 1, FirstName = "Max", LastName = "Mustermann", Email = "max@example.com" },
                new AthleteDto { Id = 2, FirstName = "Anna", LastName = "Schmidt", Email = "anna@example.com" }
            };
            _mockAthleteService.Setup(s => s.GetAllAthletesAsync())
                              .ReturnsAsync(expectedAthletes);

            // Act
            var result = await _controller.GetAll();

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
            _mockAthleteService.Setup(s => s.GetAllAthletesAsync())
                              .ReturnsAsync(expectedAthletes);

            // Act
            var result = await _controller.GetAll();

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
                Email = "max@example.com"
            };
            _mockAthleteService.Setup(s => s.GetAthleteByIdAsync(athleteId))
                              .ReturnsAsync(expectedAthlete);

            // Act
            var result = await _controller.GetById(athleteId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var athlete = Assert.IsType<AthleteDto>(okResult.Value);
            Assert.Equal(expectedAthlete.Id, athlete.Id);
            Assert.Equal(expectedAthlete.FirstName, athlete.FirstName);
            Assert.Equal(expectedAthlete.LastName, athlete.LastName);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var athleteId = 999;
            _mockAthleteService.Setup(s => s.GetAthleteByIdAsync(athleteId))
                              .ReturnsAsync((AthleteDto)null);

            // Act
            var result = await _controller.GetById(athleteId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal($"Athlet mit ID {athleteId} wurde nicht gefunden.", notFoundResult.Value);
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
                Email = "max@example.com"
            };
            var createdAthlete = new AthleteDto
            {
                Id = 1,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email
            };
            _mockAthleteService.Setup(s => s.CreateAthleteAsync(createDto))
                              .ReturnsAsync(createdAthlete);

            // Act
            var result = await _controller.Create(createDto);

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
                Email = "max.updated@example.com"
            };
            _mockAthleteService.Setup(s => s.UpdateAthleteAsync(updateDto))
                              .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockAthleteService.Verify(s => s.UpdateAthleteAsync(updateDto), Times.Once);
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
                Email = "max@example.com"
            };

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID in der URL stimmt nicht mit der ID im Körper überein.", badRequestResult.Value);
            _mockAthleteService.Verify(s => s.UpdateAthleteAsync(It.IsAny<UpdateAthleteDto>()), Times.Never);
        }

        [Fact]
        public async Task Update_WhenServiceThrowsException_ReturnsNotFound()
        {
            // Arrange
            var id = 1;
            var updateDto = new UpdateAthleteDto
            {
                Id = id,
                FirstName = "Max",
                LastName = "Mustermann",
                Email = "max@example.com"
            };
            var exceptionMessage = "Athlete not found";
            _mockAthleteService.Setup(s => s.UpdateAthleteAsync(updateDto))
                              .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.Update(id, updateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(exceptionMessage, notFoundResult.Value);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var id = 1;
            _mockAthleteService.Setup(s => s.DeleteAthleteAsync(id))
                              .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockAthleteService.Verify(s => s.DeleteAthleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task Delete_WhenServiceThrowsException_ReturnsNotFound()
        {
            // Arrange
            var id = 1;
            var exceptionMessage = "Athlete not found";
            _mockAthleteService.Setup(s => s.DeleteAthleteAsync(id))
                              .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.Delete(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(exceptionMessage, notFoundResult.Value);
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
                Email = "strava.user@example.com"
            };
            _mockAthleteService.Setup(s => s.ImportAthleteFromStravaAsync(accessToken))
                              .ReturnsAsync(importedAthlete);

            // Act
            var result = await _controller.ImportFromStrava(accessToken);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var athlete = Assert.IsType<AthleteDto>(okResult.Value);
            Assert.Equal(importedAthlete.Id, athlete.Id);
            Assert.Equal(importedAthlete.FirstName, athlete.FirstName);
        }

        [Fact]
        public async Task ImportFromStrava_WithInvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var accessToken = "invalid_token";
            var exceptionMessage = "Invalid Strava access token";
            _mockAthleteService.Setup(s => s.ImportAthleteFromStravaAsync(accessToken))
                              .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller.ImportFromStrava(accessToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(exceptionMessage, badRequestResult.Value);
        }

        [Fact]
        public async Task ImportFromStrava_WithNullToken_ReturnsBadRequest()
        {
            // Arrange
            string accessToken = null;
            var exceptionMessage = "Access token is required";
            _mockAthleteService.Setup(s => s.ImportAthleteFromStravaAsync(accessToken))
                              .ThrowsAsync(new ArgumentNullException(exceptionMessage));

            // Act
            var result = await _controller.ImportFromStrava(accessToken);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains(exceptionMessage, badRequestResult.Value.ToString());
        }

        #endregion
    }
}