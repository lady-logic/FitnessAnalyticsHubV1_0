using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Mapping;
using FitnessAnalyticsHub.Application.Services;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Exceptions.Athletes;
using FitnessAnalyticsHub.Domain.Interfaces;
using Moq;

namespace FitnessAnalyticsHub.Tests.Services;

public class AthleteServiceTests
{
    private readonly Mock<IRepository<Athlete>> _mockAthleteRepository;
    private readonly Mock<IStravaService> _mockStravaService;
    private readonly IMapper _mapper;
    private readonly AthleteService _athleteService;

    public AthleteServiceTests()
    {
        _mockAthleteRepository = new Mock<IRepository<Athlete>>();
        _mockStravaService = new Mock<IStravaService>();

        // Konfiguriere AutoMapper mit dem tatsächlichen Mappingprofil
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _athleteService = new AthleteService(
            _mockAthleteRepository.Object,
            _mockStravaService.Object,
            _mapper);
    }

    [Fact]
    public async Task GetAthleteByIdAsync_ShouldReturnAthlete_WhenAthleteExists()
    {
        // Arrange
        var athleteId = 1;
        var athlete = new Athlete
        {
            Id = athleteId,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@example.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(athlete);

        // Act
        var result = await _athleteService.GetAthleteByIdAsync(athleteId, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(athleteId, result.Id);
        Assert.Equal("Max", result.FirstName);
        Assert.Equal("Mustermann", result.LastName);
        Assert.Equal("max@example.com", result.Email);
    }

    [Fact]
    public async Task GetAthleteByIdAsync_ShouldThrowAthleteNotFoundException_WhenAthleteDoesNotExist()
    {
        // Arrange
        var athleteId = 999;
        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Athlete)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AthleteNotFoundException>(
            () => _athleteService.GetAthleteByIdAsync(athleteId, It.IsAny<CancellationToken>()));

        Assert.Equal(athleteId, exception.AthleteId);
    }

    [Fact]
    public async Task GetAllAthletesAsync_ShouldReturnAllAthletes()
    {
        // Arrange
        var athletes = new List<Athlete>
        {
            new Athlete
            {
                Id = 1,
                FirstName = "Max",
                LastName = "Mustermann",
                Email = "max@example.com",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new Athlete
            {
                Id = 2,
                FirstName = "Anna",
                LastName = "Schmidt",
                Email = "anna@example.com",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };

        _mockAthleteRepository.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(athletes);

        // Act
        var result = await _athleteService.GetAllAthletesAsync(It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Equal("Max", result.ElementAt(0).FirstName);
        Assert.Equal("Anna", result.ElementAt(1).FirstName);
    }

    [Fact]
    public async Task CreateAthleteAsync_ShouldCreateAndReturnAthlete()
    {
        // Arrange
        var createAthleteDto = new CreateAthleteDto
        {
            FirstName = "Lisa",
            LastName = "Müller",
            Email = "lisa@example.com",
            City = "Berlin",
            Country = "Germany"
        };

        Athlete createdAthlete = null;

        _mockAthleteRepository.Setup(repo => repo.AddAsync(It.IsAny<Athlete>(), It.IsAny<CancellationToken>()))
            .Callback<Athlete, CancellationToken>((athlete, token) =>
            {
                athlete.Id = 3; // Simuliere Datenbankgenerierung der ID
                createdAthlete = athlete;
            });

        // Act
        var result = await _athleteService.CreateAthleteAsync(createAthleteDto, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("Lisa", result.FirstName);
        Assert.Equal("Müller", result.LastName);
        Assert.Equal("lisa@example.com", result.Email);

        _mockAthleteRepository.Verify(repo => repo.AddAsync(It.IsAny<Athlete>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAthleteRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAthleteAsync_ShouldUpdateAthlete_WhenAthleteExists()
    {
        // Arrange
        var updateAthleteDto = new UpdateAthleteDto
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann-Update",
            Email = "max_updated@example.com",
            City = "München",
            Country = "Germany"
        };

        var existingAthlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@example.com",
            City = "Berlin",
            Country = "Germany",
            CreatedAt = DateTime.Now.AddDays(-10),
            UpdatedAt = DateTime.Now.AddDays(-10)
        };

        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(updateAthleteDto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAthlete);

        // Act
        await _athleteService.UpdateAthleteAsync(updateAthleteDto, It.IsAny<CancellationToken>());

        // Assert
        _mockAthleteRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Athlete>()), Times.Once);
        _mockAthleteRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Überprüfen, dass die Eigenschaften korrekt aktualisiert wurden
        Assert.Equal("Mustermann-Update", existingAthlete.LastName);
        Assert.Equal("max_updated@example.com", existingAthlete.Email);
        Assert.Equal("München", existingAthlete.City);
    }

    [Fact]
    public async Task UpdateAthleteAsync_ShouldThrowAthleteNotFoundException_WhenAthleteDoesNotExist()
    {
        // Arrange
        var updateAthleteDto = new UpdateAthleteDto
        {
            Id = 999,
            FirstName = "Nicht",
            LastName = "Vorhanden"
        };

        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(updateAthleteDto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Athlete)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AthleteNotFoundException>(
            () => _athleteService.UpdateAthleteAsync(updateAthleteDto, It.IsAny<CancellationToken>()));

        Assert.Equal(999, exception.AthleteId);
        _mockAthleteRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Athlete>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAthleteAsync_ShouldDeleteAthlete_WhenAthleteExists()
    {
        // Arrange
        var athleteId = 1;
        var existingAthlete = new Athlete { Id = athleteId };

        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAthlete);

        // Act
        await _athleteService.DeleteAthleteAsync(athleteId, It.IsAny<CancellationToken>());

        // Assert
        _mockAthleteRepository.Verify(repo => repo.DeleteAsync(existingAthlete), Times.Once);
        _mockAthleteRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAthleteAsync_ShouldThrowAthleteNotFoundException_WhenAthleteDoesNotExist()
    {
        // Arrange
        var athleteId = 999;
        _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Athlete)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AthleteNotFoundException>(
            () => _athleteService.DeleteAthleteAsync(athleteId, It.IsAny<CancellationToken>()));

        Assert.Equal(999, exception.AthleteId);
        _mockAthleteRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Athlete>()), Times.Never);
    }

    [Fact]
    public async Task ImportAthleteFromStravaAsync_ShouldCreateNewAthlete_WhenAthleteDoesNotExist()
    {
        // Arrange
        var accessToken = "test_token";
        var stravaAthlete = new Athlete
        {
            StravaId = "12345",
            FirstName = "Strava",
            LastName = "Athlete",
            Email = "strava@example.com"
        };

        _mockStravaService.Setup(service => service.GetAthleteProfileAsync(accessToken))
            .ReturnsAsync(stravaAthlete);

        _mockAthleteRepository.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Athlete, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Athlete>());

        Athlete createdAthlete = null;
        _mockAthleteRepository.Setup(repo => repo.AddAsync(It.IsAny<Athlete>(), It.IsAny<CancellationToken>()))
            .Callback<Athlete, CancellationToken>((athlete, token) =>
            {
                athlete.Id = 3; // Simuliere Datenbankgenerierung der ID
                createdAthlete = athlete;
            });

        // Act
        var result = await _athleteService.ImportAthleteFromStravaAsync(accessToken, It.IsAny<CancellationToken>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
        Assert.Equal("12345", result.StravaId);
        Assert.Equal("Strava", result.FirstName);
        Assert.Equal("Athlete", result.LastName);
        Assert.Equal("strava@example.com", result.Email);

        _mockAthleteRepository.Verify(repo => repo.AddAsync(It.IsAny<Athlete>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAthleteRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}