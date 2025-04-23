using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Mapping;
using FitnessAnalyticsHub.Application.Services;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FitnessAnalyticsHub.Tests.Services
{
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

            _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId))
                .ReturnsAsync(athlete);

            // Act
            var result = await _athleteService.GetAthleteByIdAsync(athleteId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(athleteId);
            result.FirstName.Should().Be("Max");
            result.LastName.Should().Be("Mustermann");
            result.Email.Should().Be("max@example.com");
        }

        [Fact]
        public async Task GetAthleteByIdAsync_ShouldReturnNull_WhenAthleteDoesNotExist()
        {
            // Arrange
            var athleteId = 999;
            _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId))
                .ReturnsAsync((Athlete)null);

            // Act
            var result = await _athleteService.GetAthleteByIdAsync(athleteId);

            // Assert
            result.Should().BeNull();
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

            _mockAthleteRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(athletes);

            // Act
            var result = await _athleteService.GetAllAthletesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.ElementAt(0).FirstName.Should().Be("Max");
            result.ElementAt(1).FirstName.Should().Be("Anna");
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

            _mockAthleteRepository.Setup(repo => repo.AddAsync(It.IsAny<Athlete>()))
                .Callback<Athlete>(athlete =>
                {
                    athlete.Id = 3; // Simuliere Datenbankgenerierung der ID
                    createdAthlete = athlete;
                });

            // Act
            var result = await _athleteService.CreateAthleteAsync(createAthleteDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(3);
            result.FirstName.Should().Be("Lisa");
            result.LastName.Should().Be("Müller");
            result.Email.Should().Be("lisa@example.com");

            _mockAthleteRepository.Verify(repo => repo.AddAsync(It.IsAny<Athlete>()), Times.Once);
            _mockAthleteRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
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

            _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(updateAthleteDto.Id))
                .ReturnsAsync(existingAthlete);

            // Act
            await _athleteService.UpdateAthleteAsync(updateAthleteDto);

            // Assert
            _mockAthleteRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Athlete>()), Times.Once);
            _mockAthleteRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            // Überprüfen, dass die Eigenschaften korrekt aktualisiert wurden
            existingAthlete.LastName.Should().Be("Mustermann-Update");
            existingAthlete.Email.Should().Be("max_updated@example.com");
            existingAthlete.City.Should().Be("München");
        }

        [Fact]
        public async Task UpdateAthleteAsync_ShouldThrowException_WhenAthleteDoesNotExist()
        {
            // Arrange
            var updateAthleteDto = new UpdateAthleteDto
            {
                Id = 999,
                FirstName = "Nicht",
                LastName = "Vorhanden"
            };

            _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(updateAthleteDto.Id))
                .ReturnsAsync((Athlete)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _athleteService.UpdateAthleteAsync(updateAthleteDto));
            _mockAthleteRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Athlete>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAthleteAsync_ShouldDeleteAthlete_WhenAthleteExists()
        {
            // Arrange
            var athleteId = 1;
            var existingAthlete = new Athlete { Id = athleteId };

            _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId))
                .ReturnsAsync(existingAthlete);

            // Act
            await _athleteService.DeleteAthleteAsync(athleteId);

            // Assert
            _mockAthleteRepository.Verify(repo => repo.DeleteAsync(existingAthlete), Times.Once);
            _mockAthleteRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAthleteAsync_ShouldThrowException_WhenAthleteDoesNotExist()
        {
            // Arrange
            var athleteId = 999;
            _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId))
                .ReturnsAsync((Athlete)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _athleteService.DeleteAthleteAsync(athleteId));
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

            _mockAthleteRepository.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Athlete, bool>>>()))
                .ReturnsAsync(new List<Athlete>());

            Athlete createdAthlete = null;
            _mockAthleteRepository.Setup(repo => repo.AddAsync(It.IsAny<Athlete>()))
                .Callback<Athlete>(athlete =>
                {
                    athlete.Id = 3; // Simuliere Datenbankgenerierung der ID
                    createdAthlete = athlete;
                });

            // Act
            var result = await _athleteService.ImportAthleteFromStravaAsync(accessToken);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(3);
            result.StravaId.Should().Be("12345");
            result.FirstName.Should().Be("Strava");
            result.LastName.Should().Be("Athlete");
            result.Email.Should().Be("strava@example.com");

            _mockAthleteRepository.Verify(repo => repo.AddAsync(It.IsAny<Athlete>()), Times.Once);
            _mockAthleteRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
