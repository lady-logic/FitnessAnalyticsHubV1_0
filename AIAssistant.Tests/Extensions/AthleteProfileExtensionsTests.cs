using AIAssistant.Application.DTOs;
using AIAssistant.Domain.Models;
using AIAssistant.Extensions;
using AIAssistant.Tests.Helpers;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using FitnessAnalyticsHub.AIAssistant.Extensions;

namespace AIAssistant.Tests.Extensions;

public class AthleteProfileExtensionsTests
{
    #region ToDto Tests

    [Fact]
    public void ToDto_WithValidDomainModel_ReturnsCorrectDto()
    {
        // Arrange
        var domain = new AthleteProfile
        {
            Id = "test-id-123",
            Name = "John Doe",
            FitnessLevel = "Advanced",
            PrimaryGoal = "Marathon Training",
            Preferences = new Dictionary<string, object>
            {
                { "preferredActivities", new[] { "Run", "Bike" } },
                { "trainingDays", 5 }
            }
        };

        // Act
        var dto = domain.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(domain.Id, dto.Id);
        Assert.Equal(domain.Name, dto.Name);
        Assert.Equal(domain.FitnessLevel, dto.FitnessLevel);
        Assert.Equal(domain.PrimaryGoal, dto.PrimaryGoal);
        Assert.Equal(domain.Preferences, dto.Preferences);
    }

    [Fact]
    public void ToDto_WithNullPreferences_ReturnsNullPreferences()
    {
        // Arrange
        var domain = new AthleteProfile
        {
            Id = "test-id",
            Name = "Test User",
            FitnessLevel = "Beginner",
            PrimaryGoal = "Get Started",
            Preferences = null
        };

        // Act
        var dto = domain.ToDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Null(dto.Preferences);
    }

    #endregion

    #region ToDomain Tests

    [Fact]
    public void ToDomain_WithValidDto_ReturnsCorrectDomainModel()
    {
        // Arrange
        var dto = TestDataBuilder.AthleteProfile()
            .WithId("dto-test-id")
            .WithName("Jane Smith")
            .WithFitnessLevel("Intermediate")
            .WithPrimaryGoal("Weight Loss")
            .Build();

        // Act
        var domain = dto.ToDomain();

        // Assert
        Assert.NotNull(domain);
        Assert.Equal(dto.Id, domain.Id);
        Assert.Equal(dto.Name, domain.Name);
        Assert.Equal(dto.FitnessLevel, domain.FitnessLevel);
        Assert.Equal(dto.PrimaryGoal, domain.PrimaryGoal);
        Assert.Equal(dto.Preferences, domain.Preferences);
    }

    [Fact]
    public void ToDomain_WithNullId_GeneratesNewId()
    {
        // Arrange
        var dto = new AthleteProfileDto
        {
            Id = null,
            Name = "Test User",
            FitnessLevel = "Beginner",
            PrimaryGoal = "General Fitness"
        };

        // Act
        var domain = dto.ToDomain();

        // Assert
        Assert.NotNull(domain);
        Assert.False(string.IsNullOrEmpty(domain.Id));
        Assert.True(Guid.TryParse(domain.Id, out _)); // Should be a valid GUID
    }

    #endregion

    #region GrpcJson ToAthleteProfileDto Tests

    [Fact]
    public void ToAthleteProfileDto_WithValidGrpcJsonProfile_ReturnsCorrectDto()
    {
        // Arrange
        var grpcProfile = TestDataBuilder.GrpcJsonAthleteProfile()
            .WithName("gRPC Test User")
            .Build();
        grpcProfile.FitnessLevel = "Expert";
        grpcProfile.PrimaryGoal = "Competition";

        // Act
        var dto = grpcProfile.ToAthleteProfileDto();

        // Assert
        Assert.NotNull(dto);
        Assert.False(string.IsNullOrEmpty(dto.Id)); // Should generate new ID
        Assert.True(Guid.TryParse(dto.Id, out _)); // Should be valid GUID
        Assert.Equal("gRPC Test User", dto.Name);
        Assert.Equal("Expert", dto.FitnessLevel);
        Assert.Equal("Competition", dto.PrimaryGoal);
        Assert.Null(dto.Preferences); // gRPC-JSON has no preferences
    }

    [Fact]
    public void ToAthleteProfileDto_WithNullName_HandlesGracefully()
    {
        // Arrange
        var grpcProfile = new GrpcJsonAthleteProfileDto
        {
            Name = null,
            FitnessLevel = "Beginner",
            PrimaryGoal = "Get Started"
        };

        // Act
        var dto = grpcProfile.ToAthleteProfileDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("", dto.Name); // Should convert null to empty string
        Assert.Equal("Beginner", dto.FitnessLevel);
        Assert.Equal("Get Started", dto.PrimaryGoal);
    }

    #endregion

    #region gRPC AthleteProfile ToAthleteProfileDto Tests

    [Fact]
    public void ToAthleteProfileDto_WithValidGrpcProfile_ReturnsCorrectDto()
    {
        // Arrange
        var grpcProfile = new global::Fitnessanalyticshub.AthleteProfile
        {
            Name = "gRPC Athlete",
            FitnessLevel = "Advanced",
            PrimaryGoal = "Triathlon Training"
        };

        // Act
        var dto = grpcProfile.ToAthleteProfileDto();

        // Assert
        Assert.NotNull(dto);
        Assert.False(string.IsNullOrEmpty(dto.Id));
        Assert.True(Guid.TryParse(dto.Id, out _));
        Assert.Equal("gRPC Athlete", dto.Name);
        Assert.Equal("Advanced", dto.FitnessLevel);
        Assert.Equal("Triathlon Training", dto.PrimaryGoal);
        Assert.Null(dto.Preferences);
    }

    [Fact]
    public void ToAthleteProfileDto_WithNullGrpcFields_HandlesGracefully()
    {
        // Arrange
        var grpcProfile = new global::Fitnessanalyticshub.AthleteProfile
        {
            Name = null,
            FitnessLevel = null,
            PrimaryGoal = null
        };

        // Act
        var dto = grpcProfile.ToAthleteProfileDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("", dto.Name);
        Assert.Equal("", dto.FitnessLevel);
        Assert.Equal("", dto.PrimaryGoal);
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ToAthleteProfileDto_WithInvalidNames_HandleGracefully(string? invalidName)
    {
        // Arrange
        var grpcProfile = new GrpcJsonAthleteProfileDto
        {
            Name = invalidName,
            FitnessLevel = "Beginner",
            PrimaryGoal = "Test"
        };

        // Act
        var dto = grpcProfile.ToAthleteProfileDto();

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(invalidName ?? "", dto.Name);
    }

    [Fact]
    public void ToDomain_PreservesComplexPreferences()
    {
        // Arrange
        var complexPreferences = new Dictionary<string, object>
        {
            { "preferredActivities", new[] { "Run", "Bike", "Swim" } },
            { "trainingDays", 6 },
            { "maxDistance", 42.2 },
            { "goals", new Dictionary<string, object>
                {
                    { "5K", "sub20" },
                    { "Marathon", "sub3" }
                }
            }
        };

        var dto = new AthleteProfileDto
        {
            Id = "complex-test",
            Name = "Complex Athlete",
            FitnessLevel = "Elite",
            PrimaryGoal = "Olympic Qualifying",
            Preferences = complexPreferences
        };

        // Act
        var domain = dto.ToDomain();
        var backToDto = domain.ToDto();

        // Assert
        Assert.Equal(complexPreferences, domain.Preferences);
        Assert.Equal(complexPreferences, backToDto.Preferences);
    }

    #endregion
}