namespace FitnessAnalyticsHub.Tests.Services;

using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Mapping;
using FitnessAnalyticsHub.Application.Services;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Exceptions.Athletes;
using FitnessAnalyticsHub.Domain.Interfaces;
using FitnessAnalyticsHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

public class AthleteServiceTests
{
    private readonly ApplicationDbContext context;
    private readonly Mock<IStravaService> mockStravaService;
    private readonly IMapper mapper;
    private readonly AthleteService athleteService;

    public AthleteServiceTests()
    {
        // InMemory Database erstellen
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new ApplicationDbContext(options);
        this.mockStravaService = new Mock<IStravaService>();

        // Konfiguriere AutoMapper mit dem tatsächlichen Mappingprofil
        MapperConfiguration mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        this.mapper = mapperConfig.CreateMapper();

        // Service erstellen
        this.athleteService = new AthleteService(
            this.context,
            this.mockStravaService.Object,
            this.mapper);
    }

    public void Dispose()
    {
        this.context.Dispose();
    }

    [Fact]
    public async Task GetAthleteByIdAsync_ShouldReturnAthlete_WhenAthleteExists()
    {
        // Arrange
        Athlete athlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            City = "Berlin",
            Country = "Germany",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        await this.context.Athletes.AddAsync(athlete);
        await this.context.SaveChangesAsync();

        // Act
        AthleteDto result = await this.athleteService.GetAthleteByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Max", result.FirstName);
        Assert.Equal("Mustermann", result.LastName);
        Assert.Equal("max@test.com", result.Email);
        Assert.Equal("Berlin", result.City);
        Assert.Equal("Germany", result.Country);
    }

    [Fact]
    public async Task GetAthleteByIdAsync_ShouldThrowAthleteNotFoundException_WhenAthleteDoesNotExist()
    {
        // Arrange - Keine Daten in DB

        // Act & Assert
        AthleteNotFoundException exception = await Assert.ThrowsAsync<AthleteNotFoundException>(
            () => this.athleteService.GetAthleteByIdAsync(999, CancellationToken.None));

        Assert.Equal(999, exception.AthleteId);
    }

    [Fact]
    public async Task GetAllAthletesAsync_ShouldReturnAllAthletes()
    {
        // Arrange
        List<Athlete> athletes = new List<Athlete>
    {
        new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        },
        new Athlete
        {
            Id = 2,
            FirstName = "Anna",
            LastName = "Schmidt",
            Email = "anna@test.com",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        },
    };

        await this.context.Athletes.AddRangeAsync(athletes);
        await this.context.SaveChangesAsync();

        // Act
        IEnumerable<AthleteDto> result = await this.athleteService.GetAllAthletesAsync(CancellationToken.None);

        // Assert
        List<AthleteDto> resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, a => a.FirstName == "Max" && a.LastName == "Mustermann");
        Assert.Contains(resultList, a => a.FirstName == "Anna" && a.LastName == "Schmidt");
    }

    [Fact]
    public async Task CreateAthleteAsync_ShouldCreateAthlete_WhenValidData()
    {
        // Arrange
        CreateAthleteDto createDto = new CreateAthleteDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@test.com",
            City = "Munich",
            Country = "Germany",
        };

        // Act
        AthleteDto result = await this.athleteService.CreateAthleteAsync(createDto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.FirstName);
        Assert.Equal("User", result.LastName);
        Assert.Equal("test@test.com", result.Email);
        Assert.True(result.Id > 0);

        // Verify in database
        Athlete? athleteInDb = await this.context.Athletes.FindAsync(result.Id);
        Assert.NotNull(athleteInDb);
        Assert.Equal("Test", athleteInDb.FirstName);
    }

    [Fact]
    public async Task UpdateAthleteAsync_ShouldUpdateAthlete_WhenAthleteExists()
    {
        // Arrange
        Athlete athlete = new Athlete
        {
            Id = 1,
            FirstName = "Max",
            LastName = "Mustermann",
            Email = "max@test.com",
            City = "Berlin",
            Country = "Germany",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        await this.context.Athletes.AddAsync(athlete);
        await this.context.SaveChangesAsync();

        UpdateAthleteDto updateDto = new UpdateAthleteDto
        {
            Id = 1,
            FirstName = "Maximilian",
            LastName = "Mustermann",
            City = "Munich",
            Country = "Germany",
        };

        // Act
        await this.athleteService.UpdateAthleteAsync(updateDto, CancellationToken.None);

        // Assert
        Athlete? updatedAthlete = await this.context.Athletes.FindAsync(1);
        Assert.NotNull(updatedAthlete);
        Assert.Equal("Maximilian", updatedAthlete.FirstName);
        Assert.Equal("Munich", updatedAthlete.City);
    }

    [Fact]
    public async Task UpdateAthleteAsync_ShouldThrowAthleteNotFoundException_WhenAthleteDoesNotExist()
    {
        // Arrange
        UpdateAthleteDto updateDto = new UpdateAthleteDto
        {
            Id = 999,
            FirstName = "Test",
            LastName = "User",
        };

        // Act & Assert
        AthleteNotFoundException exception = await Assert.ThrowsAsync<AthleteNotFoundException>(
            () => this.athleteService.UpdateAthleteAsync(updateDto, CancellationToken.None));

        Assert.Equal(999, exception.AthleteId);
    }

    [Fact]
    public async Task DeleteAthleteAsync_ShouldDeleteAthlete_WhenAthleteExists()
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
        await this.athleteService.DeleteAthleteAsync(1, CancellationToken.None);

        // Assert
        Athlete? deletedAthlete = await this.context.Athletes.FindAsync(1);
        Assert.Null(deletedAthlete);
    }

    [Fact]
    public async Task DeleteAthleteAsync_ShouldThrowAthleteNotFoundException_WhenAthleteDoesNotExist()
    {
        // Arrange - Keine Daten in DB

        // Act & Assert
        AthleteNotFoundException exception = await Assert.ThrowsAsync<AthleteNotFoundException>(
            () => this.athleteService.DeleteAthleteAsync(999, CancellationToken.None));

        Assert.Equal(999, exception.AthleteId);
    }

    [Fact]
    public async Task ImportAthleteFromStravaAsync_ShouldCreateNewAthlete_WhenAthleteDoesNotExist()
    {
        // Arrange
        Athlete stravaAthlete = new Athlete
        {
            StravaId = "12345",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@strava.com",
            City = "San Francisco",
            Country = "USA",
            ProfilePictureUrl = "https://strava.com/profile.jpg",
        };

        this.mockStravaService.Setup(s => s.GetAthleteProfileAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(stravaAthlete);

        // Act
        AthleteDto result = await this.athleteService.ImportAthleteFromStravaAsync("test_token", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.Equal("john@strava.com", result.Email);
        Assert.Equal("johndoe", result.Username);
        Assert.True(result.Id > 0);

        // Verify in database
        Athlete? athleteInDb = await this.context.Athletes.FirstOrDefaultAsync(a => a.StravaId == "12345");
        Assert.NotNull(athleteInDb);
        Assert.Equal("12345", athleteInDb.StravaId);
        Assert.Equal("John", athleteInDb.FirstName);
    }

    [Fact]
    public async Task ImportAthleteFromStravaAsync_ShouldUpdateExistingAthlete_WhenAthleteAlreadyExists()
    {
        // Arrange
        Athlete existingAthlete = new Athlete
        {
            Id = 1,
            StravaId = "12345",
            FirstName = "John",
            LastName = "Doe",
            Email = "old@email.com",
            City = "Old City",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        await this.context.Athletes.AddAsync(existingAthlete);
        await this.context.SaveChangesAsync();

        Athlete stravaAthlete = new Athlete
        {
            StravaId = "12345",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe_updated",
            Email = "new@strava.com",
            City = "San Francisco",
            Country = "USA",
            ProfilePictureUrl = "https://strava.com/new_profile.jpg",
        };

        this.mockStravaService.Setup(s => s.GetAthleteProfileAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(stravaAthlete);

        // Act
        AthleteDto result = await this.athleteService.ImportAthleteFromStravaAsync("test_token", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id); // Sollte die gleiche ID haben
        Assert.Equal("new@strava.com", result.Email); // Email sollte aktualisiert sein
        Assert.Equal("San Francisco", result.City); // City sollte aktualisiert sein
        Assert.Equal("johndoe_updated", result.Username); // Username sollte aktualisiert sein

        // Verify in database - sollte nur einen Athlete geben
        List<Athlete> athletesInDb = await this.context.Athletes.Where(a => a.StravaId == "12345").ToListAsync();
        Assert.Single(athletesInDb);
        Assert.Equal("new@strava.com", athletesInDb[0].Email);
        Assert.Equal("johndoe_updated", athletesInDb[0].Username);
    }
}