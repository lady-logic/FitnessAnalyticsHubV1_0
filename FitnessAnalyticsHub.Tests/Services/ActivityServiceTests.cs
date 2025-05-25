using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Application.Mapping;
using FitnessAnalyticsHub.Application.Services;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace FitnessAnalyticsHub.Tests.Services
{
    public class ActivityServiceTests
    {
        private readonly Mock<IRepository<Activity>> _mockActivityRepository;
        private readonly Mock<IRepository<Athlete>> _mockAthleteRepository;
        private readonly Mock<IStravaService> _mockStravaService;
        private readonly Mock<IAIAssistantClient> _mockAiAssistantClient;
        private readonly IMapper _mapper;
        private readonly ActivityService _activityService;

        public ActivityServiceTests()
        {
            _mockActivityRepository = new Mock<IRepository<Activity>>();
            _mockAthleteRepository = new Mock<IRepository<Athlete>>();
            _mockStravaService = new Mock<IStravaService>();
            _mockAiAssistantClient = new Mock<IAIAssistantClient>();

            // Konfiguriere AutoMapper mit dem tatsächlichen Mappingprofil
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _activityService = new ActivityService(
                _mockActivityRepository.Object,
                _mockAthleteRepository.Object,
                _mockStravaService.Object,
                _mockAiAssistantClient.Object,
                _mapper);
        }

        [Fact]
        public async Task GetActivityByIdAsync_ShouldReturnActivity_WhenActivityExists()
        {
            // Arrange
            var activityId = 1;
            var activity = new Activity
            {
                Id = activityId,
                AthleteId = 1,
                Name = "Morning Run",
                Distance = 5000,
                MovingTime = 1800,
                ElapsedTime = 1900,
                SportType = "Run",
                StartDate = DateTime.Now.AddDays(-1),
                StartDateLocal = DateTime.Now.AddDays(-1),
                Athlete = new Athlete { FirstName = "Max", LastName = "Mustermann" }
            };

            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId))
                .ReturnsAsync(activity);

            // Act
            var result = await _activityService.GetActivityByIdAsync(activityId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(activityId);
            result.Name.Should().Be("Morning Run");
            result.Distance.Should().Be(5000);
            result.SportType.Should().Be("Run");
            result.AthleteFullName.Should().Be("Max Mustermann");
        }

        [Fact]
        public async Task GetActivityByIdAsync_ShouldReturnNull_WhenActivityDoesNotExist()
        {
            // Arrange
            var activityId = 999;
            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId))
                .ReturnsAsync((Activity)null);

            // Act
            var result = await _activityService.GetActivityByIdAsync(activityId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetActivitiesByAthleteIdAsync_ShouldReturnAthleteActivities()
        {
            // Arrange
            var athleteId = 1;
            var athlete = new Athlete
            {
                Id = athleteId,
                FirstName = "Max",
                LastName = "Mustermann"
            };

            var activities = new List<Activity>
            {
                new Activity
                {
                    Id = 1,
                    AthleteId = athleteId,
                    Name = "Morning Run",
                    Distance = 5000,
                    MovingTime = 1800,
                    ElapsedTime = 1900,
                    SportType = "Run",
                    StartDate = DateTime.Now.AddDays(-1),
                    StartDateLocal = DateTime.Now.AddDays(-1)
                },
                new Activity
                {
                    Id = 2,
                    AthleteId = athleteId,
                    Name = "Evening Ride",
                    Distance = 20000,
                    MovingTime = 3600,
                    ElapsedTime = 3700,
                    SportType = "Ride",
                    StartDate = DateTime.Now.AddDays(-2),
                    StartDateLocal = DateTime.Now.AddDays(-2)
                }
            };

            _mockActivityRepository.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Activity, bool>>>()))
                .ReturnsAsync(activities);

            _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId))
                .ReturnsAsync(athlete);

            // Act
            var result = await _activityService.GetActivitiesByAthleteIdAsync(athleteId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.ElementAt(0).Name.Should().Be("Morning Run");
            result.ElementAt(1).Name.Should().Be("Evening Ride");
            result.ElementAt(0).AthleteFullName.Should().Be("Max Mustermann");
            result.ElementAt(1).AthleteFullName.Should().Be("Max Mustermann");
        }

        [Fact]
        public async Task CreateActivityAsync_ShouldCreateAndReturnActivity()
        {
            // Arrange
            var athleteId = 1;
            var createActivityDto = new CreateActivityDto
            {
                AthleteId = athleteId,
                Name = "Test Activity",
                Distance = 10000,
                MovingTimeSeconds = 2400,
                ElapsedTimeSeconds = 2500,
                SportType = "Run",
                StartDate = DateTime.Now,
                StartDateLocal = DateTime.Now
            };

            var athlete = new Athlete
            {
                Id = athleteId,
                FirstName = "Max",
                LastName = "Mustermann"
            };

            Activity createdActivity = null;

            _mockActivityRepository.Setup(repo => repo.AddAsync(It.IsAny<Activity>()))
                .Callback<Activity>(activity =>
                {
                    activity.Id = 3; // Simuliere Datenbankgenerierung der ID
                    createdActivity = activity;
                });

            _mockAthleteRepository.Setup(repo => repo.GetByIdAsync(athleteId))
                .ReturnsAsync(athlete);

            // Act
            var result = await _activityService.CreateActivityAsync(createActivityDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(3);
            result.Name.Should().Be("Test Activity");
            result.Distance.Should().Be(10000);
            result.MovingTime.TotalSeconds.Should().Be(2400);
            result.AthleteFullName.Should().Be("Max Mustermann");

            _mockActivityRepository.Verify(repo => repo.AddAsync(It.IsAny<Activity>()), Times.Once);
            _mockActivityRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateActivityAsync_ShouldUpdateActivity_WhenActivityExists()
        {
            // Arrange
            var updateActivityDto = new UpdateActivityDto
            {
                Id = 1,
                Name = "Updated Activity",
                Distance = 11000,
                MovingTimeSeconds = 2500,
                ElapsedTimeSeconds = 2600,
                SportType = "Run",
                StartDate = DateTime.Now,
                StartDateLocal = DateTime.Now
            };

            var existingActivity = new Activity
            {
                Id = 1,
                AthleteId = 1,
                Name = "Original Activity",
                Distance = 10000,
                MovingTime = 2400,
                ElapsedTime = 2500,
                SportType = "Run",
                StartDate = DateTime.Now.AddDays(-1),
                StartDateLocal = DateTime.Now.AddDays(-1),
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now.AddDays(-1)
            };

            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(updateActivityDto.Id))
                .ReturnsAsync(existingActivity);

            // Act
            await _activityService.UpdateActivityAsync(updateActivityDto);

            // Assert
            _mockActivityRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Activity>()), Times.Once);
            _mockActivityRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);

            // Überprüfen, dass die Eigenschaften korrekt aktualisiert wurden
            existingActivity.Name.Should().Be("Updated Activity");
            existingActivity.Distance.Should().Be(11000);
            existingActivity.MovingTime.Should().Be(2500);
        }

        [Fact]
        public async Task UpdateActivityAsync_ShouldThrowException_WhenActivityDoesNotExist()
        {
            // Arrange
            var updateActivityDto = new UpdateActivityDto
            {
                Id = 999,
                Name = "Non-existent Activity"
            };

            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(updateActivityDto.Id))
                .ReturnsAsync((Activity)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _activityService.UpdateActivityAsync(updateActivityDto));
            _mockActivityRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Activity>()), Times.Never);
        }

        [Fact]
        public async Task DeleteActivityAsync_ShouldDeleteActivity_WhenActivityExists()
        {
            // Arrange
            var activityId = 1;
            var existingActivity = new Activity { Id = activityId };

            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId))
                .ReturnsAsync(existingActivity);

            // Act
            await _activityService.DeleteActivityAsync(activityId);

            // Assert
            _mockActivityRepository.Verify(repo => repo.DeleteAsync(existingActivity), Times.Once);
            _mockActivityRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteActivityAsync_ShouldThrowException_WhenActivityDoesNotExist()
        {
            // Arrange
            var activityId = 999;
            _mockActivityRepository.Setup(repo => repo.GetByIdAsync(activityId))
                .ReturnsAsync((Activity)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _activityService.DeleteActivityAsync(activityId));
            _mockActivityRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Activity>()), Times.Never);
        }

        [Fact]
        public async Task GetAthleteActivityStatisticsAsync_ShouldReturnCorrectStatistics()
        {
            // Arrange
            var athleteId = 1;
            var activities = new List<Activity>
            {
                new Activity
                {
                    Id = 1,
                    AthleteId = athleteId,
                    Name = "Morning Run",
                    Distance = 5000,
                    MovingTime = 1800,
                    ElapsedTime = 1900,
                    TotalElevationGain = 100,
                    SportType = "Run",
                    StartDate = new DateTime(2023, 1, 15),
                    StartDateLocal = new DateTime(2023, 1, 15)
                },
                new Activity
                {
                    Id = 2,
                    AthleteId = athleteId,
                    Name = "Evening Ride",
                    Distance = 20000,
                    MovingTime = 3600,
                    ElapsedTime = 3700,
                    TotalElevationGain = 300,
                    SportType = "Ride",
                    StartDate = new DateTime(2023, 2, 15),
                    StartDateLocal = new DateTime(2023, 2, 15)
                },
                new Activity
                {
                    Id = 3,
                    AthleteId = athleteId,
                    Name = "Weekend Run",
                    Distance = 10000,
                    MovingTime = 3000,
                    ElapsedTime = 3100,
                    TotalElevationGain = 150,
                    SportType = "Run",
                    StartDate = new DateTime(2023, 2, 20),
                    StartDateLocal = new DateTime(2023, 2, 20)
                }
            };

            _mockActivityRepository.Setup(repo => repo.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Activity, bool>>>()))
                .ReturnsAsync(activities);

            // Act
            var result = await _activityService.GetAthleteActivityStatisticsAsync(athleteId);

            // Assert
            result.Should().NotBeNull();
            result.TotalActivities.Should().Be(3);
            result.TotalDistance.Should().Be(35000); // 5000 + 20000 + 10000
            result.TotalDuration.TotalSeconds.Should().Be(8400); // 1800 + 3600 + 3000
            result.TotalElevationGain.Should().Be(550); // 100 + 300 + 150

            // Überprüfe die Aktivitäten nach Typ
            result.ActivitiesByType.Should().ContainKey("Run").WhoseValue.Should().Be(2);
            result.ActivitiesByType.Should().ContainKey("Ride").WhoseValue.Should().Be(1);

            // Überprüfe die Aktivitäten nach Monat
            result.ActivitiesByMonth.Should().ContainKey(1).WhoseValue.Should().Be(1); // Januar
            result.ActivitiesByMonth.Should().ContainKey(2).WhoseValue.Should().Be(2); // Februar
        }
    }
}
