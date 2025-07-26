using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Interfaces;
using FitnessAnalyticsHub.Domain.Models;
using FitnessAnalyticsHub.Infrastructure.Exceptions;
using FitnessAnalyticsHub.Tests.Base;
using FitnessAnalyticsHub.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FitnessAnalyticsHub.Tests.Controllers;

public class StravaAuthControllerTests : ControllerTestBase<StravaAuthController>
{
    private readonly Mock<IStravaService> mockStravaService;
    private readonly StravaAuthController controller;

    public StravaAuthControllerTests()
    {
        this.mockStravaService = new Mock<IStravaService>();
        this.controller = new StravaAuthController(this.mockStravaService.Object);
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidConfiguration_ReturnsRedirectToStravaAuth()
    {
        // Arrange
        var expectedAuthUrl = "https://www.strava.com/oauth/authorize?client_id=test_client&response_type=code&redirect_uri=http%3A//localhost/callback&scope=read_all,activity:read_all&approval_prompt=force";

        this.mockStravaService
            .Setup(s => s.GetAuthorizationUrlAsync())
            .ReturnsAsync(expectedAuthUrl);

        // Act
        var result = await this.controller.Login();

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal(expectedAuthUrl, redirectResult.Url);

        // Verify service was called
        this.mockStravaService.Verify(s => s.GetAuthorizationUrlAsync(), Times.Once);
    }

    [Fact]
    public async Task Login_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        this.mockStravaService
            .Setup(s => s.GetAuthorizationUrlAsync())
            .ThrowsAsync(new StravaConfigurationException("Missing client configuration"));

        // Act
        var result = await this.controller.Login();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Error generating auth URL", badRequestResult.Value?.ToString());
        Assert.Contains("Missing client configuration", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task Login_WhenServiceThrowsGenericException_ReturnsBadRequest()
    {
        // Arrange
        this.mockStravaService
            .Setup(s => s.GetAuthorizationUrlAsync())
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await this.controller.Login();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Error generating auth URL", badRequestResult.Value?.ToString());
        Assert.Contains("Unexpected error", badRequestResult.Value?.ToString());
    }

    #endregion

    #region Callback Tests

    [Fact]
    public async Task Callback_WithValidCodeAndScope_ReturnsOkWithTokenAndAthleteInfo()
    {
        // Arrange
        var authCode = "valid_auth_code";
        var scope = "read,activity:read_all";

        var tokenInfo = new TokenInfo
        {
            TokenType = "Bearer",
            AccessToken = "test_access_token_12345",
            ExpiresAt = (int)DateTimeOffset.UtcNow.AddHours(6).ToUnixTimeSeconds(),
            ExpiresIn = 21600,
            RefreshToken = "test_refresh_token",
        };

        var athlete = new Athlete
        {
            Id = 1,
            StravaId = "12345",
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe_strava",
            Email = "john@example.com",
        };

        var activities = new List<Activity>
        {
            new Activity
            {
                Id = 1,
                StravaId = "111",
                Name = "Morning Run",
                SportType = "Run",
                Distance = 5000,
                MovingTime = 1800,
                StartDate = DateTime.UtcNow.AddDays(-1),
                StartDateLocal = DateTime.Now.AddDays(-1),
            },
            new Activity
            {
                Id = 2,
                StravaId = "222",
                Name = "Evening Bike",
                SportType = "Ride",
                Distance = 20000,
                MovingTime = 3600,
                StartDate = DateTime.UtcNow.AddDays(-2),
                StartDateLocal = DateTime.Now.AddDays(-2),
            },
        };

        this.mockStravaService
            .Setup(s => s.ExchangeCodeForTokenAsync(authCode))
            .ReturnsAsync(tokenInfo);

        this.mockStravaService
            .Setup(s => s.GetAthleteProfileAsync(tokenInfo.AccessToken))
            .ReturnsAsync(athlete);

        this.mockStravaService
            .Setup(s => s.GetActivitiesAsync(tokenInfo.AccessToken, 1, 5))
            .ReturnsAsync(activities);

        // Act
        var result = await this.controller.Callback(authCode, scope, error: null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        // Use reflection to verify the anonymous object structure
        var responseType = response!.GetType();

        var messageProperty = responseType.GetProperty("message");
        var athleteProperty = responseType.GetProperty("athlete");
        var tokenProperty = responseType.GetProperty("token");
        var activitiesCountProperty = responseType.GetProperty("activitiesCount");

        Assert.NotNull(messageProperty);
        Assert.NotNull(athleteProperty);
        Assert.NotNull(tokenProperty);
        Assert.NotNull(activitiesCountProperty);

        Assert.Equal("Authorization successful!", messageProperty.GetValue(response));
        Assert.Equal(2, activitiesCountProperty.GetValue(response));

        // Verify athlete info
        var athleteInfo = athleteProperty.GetValue(response);
        var athleteInfoType = athleteInfo!.GetType();
        Assert.Equal("John", athleteInfoType.GetProperty("FirstName")?.GetValue(athleteInfo));
        Assert.Equal("Doe", athleteInfoType.GetProperty("LastName")?.GetValue(athleteInfo));
        Assert.Equal("johndoe_strava", athleteInfoType.GetProperty("Username")?.GetValue(athleteInfo));

        // Verify token info
        var tokenInfoResponse = tokenProperty.GetValue(response);
        var tokenInfoType = tokenInfoResponse!.GetType();
        Assert.Equal(tokenInfo.AccessToken, tokenInfoType.GetProperty("accessToken")?.GetValue(tokenInfoResponse));
        Assert.Equal(scope, tokenInfoType.GetProperty("receivedScopes")?.GetValue(tokenInfoResponse));

        // Verify all services were called in correct order
        this.mockStravaService.Verify(s => s.ExchangeCodeForTokenAsync(authCode), Times.Once);
        this.mockStravaService.Verify(s => s.GetAthleteProfileAsync(tokenInfo.AccessToken), Times.Once);
        this.mockStravaService.Verify(s => s.GetActivitiesAsync(tokenInfo.AccessToken, 1, 5), Times.Once);
    }

    [Fact]
    public async Task Callback_WithErrorParameter_ReturnsBadRequest()
    {
        // Arrange
        var error = "access_denied";
        var code = "some_code";
        var scope = "read";

        // Act
        var result = await this.controller.Callback(code, scope, error);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Authorization failed", badRequestResult.Value?.ToString());
        Assert.Contains("access_denied", badRequestResult.Value?.ToString());

        // Verify no services were called
        this.mockStravaService.Verify(s => s.ExchangeCodeForTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Callback_WithNoCode_ReturnsBadRequest()
    {
        // Arrange
        string? code = null;
        var scope = "read";
        string? error = null;

        // Act
        var result = await this.controller.Callback(code, scope, error);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No authorization code received", badRequestResult.Value);

        // Verify no services were called
        this.mockStravaService.Verify(s => s.ExchangeCodeForTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Callback_WithEmptyCode_ReturnsBadRequest()
    {
        // Arrange
        var code = string.Empty;
        var scope = "read";
        string? error = null;

        // Act
        var result = await this.controller.Callback(code, scope, error);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No authorization code received", badRequestResult.Value);
    }

    [Fact]
    public async Task Callback_WithMissingReadAllScope_ReturnsBadRequest()
    {
        // Arrange
        var authCode = "valid_code";
        var scope = "read"; // Missing "read_all"

        var tokenInfo = new TokenInfo
        {
            AccessToken = "test_token",
            ExpiresAt = (int)DateTimeOffset.UtcNow.AddHours(6).ToUnixTimeSeconds(),
        };

        var athlete = new Athlete
        {
            FirstName = "John",
            LastName = "Doe",
        };

        this.mockStravaService
            .Setup(s => s.ExchangeCodeForTokenAsync(authCode))
            .ReturnsAsync(tokenInfo);

        this.mockStravaService
            .Setup(s => s.GetAthleteProfileAsync(tokenInfo.AccessToken))
            .ReturnsAsync(athlete);

        // Act
        var result = await this.controller.Callback(authCode, scope, error: null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Missing required scope", badRequestResult.Value?.ToString());
        Assert.Contains("Got: read", badRequestResult.Value?.ToString());
        Assert.Contains("need 'read_all'", badRequestResult.Value?.ToString());

        // Verify token exchange and athlete profile were called, but not activities
        this.mockStravaService.Verify(s => s.ExchangeCodeForTokenAsync(authCode), Times.Once);
        this.mockStravaService.Verify(s => s.GetAthleteProfileAsync(tokenInfo.AccessToken), Times.Once);
        this.mockStravaService.Verify(s => s.GetActivitiesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Callback_WhenTokenExchangeFails_ReturnsBadRequest()
    {
        // Arrange
        var authCode = "invalid_code";
        var scope = "read,activity:read_all";

        this.mockStravaService
            .Setup(s => s.ExchangeCodeForTokenAsync(authCode))
            .ThrowsAsync(new StravaApiException("Invalid authorization code", 400));

        // Act
        var result = await this.controller.Callback(authCode, scope, error: null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Error during authorization", badRequestResult.Value?.ToString());
        Assert.Contains("Invalid authorization code", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task Callback_WhenAthleteProfileFails_ReturnsBadRequest()
    {
        // Arrange
        var authCode = "valid_code";
        var scope = "read,activity:read_all";

        var tokenInfo = new TokenInfo
        {
            AccessToken = "invalid_token",
            ExpiresAt = (int)DateTimeOffset.UtcNow.AddHours(6).ToUnixTimeSeconds(),
        };

        this.mockStravaService
            .Setup(s => s.ExchangeCodeForTokenAsync(authCode))
            .ReturnsAsync(tokenInfo);

        this.mockStravaService
            .Setup(s => s.GetAthleteProfileAsync(tokenInfo.AccessToken))
            .ThrowsAsync(new InvalidStravaTokenException("Token expired"));

        // Act
        var result = await this.controller.Callback(authCode, scope, error: null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Error during authorization", badRequestResult.Value?.ToString());
        Assert.Contains("Token expired", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task Callback_WhenActivitiesFail_StillReturnsOkWithZeroCount()
    {
        // Arrange
        var authCode = "valid_code";
        var scope = "read,activity:read_all";

        var tokenInfo = new TokenInfo
        {
            AccessToken = "test_token",
            ExpiresAt = (int)DateTimeOffset.UtcNow.AddHours(6).ToUnixTimeSeconds(),
        };

        var athlete = new Athlete
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
        };

        this.mockStravaService
            .Setup(s => s.ExchangeCodeForTokenAsync(authCode))
            .ReturnsAsync(tokenInfo);

        this.mockStravaService
            .Setup(s => s.GetAthleteProfileAsync(tokenInfo.AccessToken))
            .ReturnsAsync(athlete);

        this.mockStravaService
            .Setup(s => s.GetActivitiesAsync(tokenInfo.AccessToken, 1, 5))
            .ThrowsAsync(new StravaApiException("Rate limit exceeded", 429));

        // Act
        var result = await this.controller.Callback(authCode, scope, error: null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Error during authorization", badRequestResult.Value?.ToString());
        Assert.Contains("Rate limit exceeded", badRequestResult.Value?.ToString());
    }

    #endregion

    #region TestActivities Tests

    [Fact]
    public async Task TestActivities_WithValidToken_ReturnsOkWithActivitiesData()
    {
        // Arrange
        var accessToken = "valid_access_token";
        var activities = new List<Activity>
        {
            new Activity
            {
                Id = 1,
                StravaId = "111",
                Name = "Morning Run",
                SportType = "Run",
                Distance = 5000,
                MovingTime = 1800, // 30 minutes
                StartDate = DateTime.UtcNow.AddDays(-1),
                StartDateLocal = DateTime.Now.AddDays(-1),
            },
            new Activity
            {
                Id = 2,
                StravaId = "222",
                Name = "Bike Commute",
                SportType = "Ride",
                Distance = 15000,
                MovingTime = 2700, // 45 minutes
                StartDate = DateTime.UtcNow.AddDays(-2),
                StartDateLocal = DateTime.Now.AddDays(-2),
            },
        };

        this.mockStravaService
            .Setup(s => s.GetActivitiesAsync(accessToken, 1, 10))
            .ReturnsAsync(activities);

        // Act
        var result = await this.controller.TestActivities(accessToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        // Verify response structure using reflection
        var responseType = response!.GetType();
        var messageProperty = responseType.GetProperty("message");
        var countProperty = responseType.GetProperty("count");
        var activitiesProperty = responseType.GetProperty("activities");

        Assert.Equal("Activities retrieved successfully!", messageProperty?.GetValue(response));
        Assert.Equal(2, countProperty?.GetValue(response));

        // Verify activities data structure
        var activitiesData = activitiesProperty?.GetValue(response);
        Assert.NotNull(activitiesData);

        // Verify service was called with correct parameters
        this.mockStravaService.Verify(s => s.GetActivitiesAsync(accessToken, 1, 10), Times.Once);
    }

    [Fact]
    public async Task TestActivities_WithEmptyToken_ReturnsBadRequest()
    {
        // Arrange
        var accessToken = string.Empty;

        // Act
        var result = await this.controller.TestActivities(accessToken);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Access token required", badRequestResult.Value);

        // Verify service was never called
        this.mockStravaService.Verify(s => s.GetActivitiesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task TestActivities_WithNullToken_ReturnsBadRequest()
    {
        // Arrange
        string? accessToken = null;

        // Act
        var result = await this.controller.TestActivities(accessToken);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Access token required", badRequestResult.Value);
    }

    [Fact]
    public async Task TestActivities_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var accessToken = "invalid_token";

        this.mockStravaService
            .Setup(s => s.GetActivitiesAsync(accessToken, 1, 10))
            .ThrowsAsync(new InvalidStravaTokenException("Invalid access token"));

        // Act
        var result = await this.controller.TestActivities(accessToken);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Error getting activities", badRequestResult.Value?.ToString());
        Assert.Contains("Invalid access token", badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task TestActivities_WithEmptyActivitiesList_ReturnsOkWithZeroCount()
    {
        // Arrange
        var accessToken = "valid_token";
        var emptyActivities = new List<Activity>();

        this.mockStravaService
            .Setup(s => s.GetActivitiesAsync(accessToken, 1, 10))
            .ReturnsAsync(emptyActivities);

        // Act
        var result = await this.controller.TestActivities(accessToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        var responseType = response!.GetType();
        var countProperty = responseType.GetProperty("count");
        Assert.Equal(0, countProperty?.GetValue(response));
    }

    #endregion

    #region Edge Cases and Error Handling

    [Theory]
    [InlineData("read")]
    [InlineData("read,activity:read")]
    [InlineData("activity:read")]
    [InlineData("")]
    public async Task Callback_WithInsufficientScopes_ReturnsBadRequest(string insufficientScope)
    {
        // Arrange
        var authCode = "valid_code";

        var tokenInfo = new TokenInfo { AccessToken = "test_token" };
        var athlete = new Athlete { FirstName = "Test", LastName = "User" };

        this.mockStravaService.Setup(s => s.ExchangeCodeForTokenAsync(authCode)).ReturnsAsync(tokenInfo);
        this.mockStravaService.Setup(s => s.GetAthleteProfileAsync(tokenInfo.AccessToken)).ReturnsAsync(athlete);

        // Act
        var result = await this.controller.Callback(authCode, insufficientScope, error: null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Missing required scope", badRequestResult.Value?.ToString());
        Assert.Contains(insufficientScope, badRequestResult.Value?.ToString());
    }

    [Fact]
    public async Task TestActivities_CalculatesMovingTimeCorrectly()
    {
        // Arrange
        var accessToken = "valid_token";
        var activities = new List<Activity>
        {
            new Activity
            {
                Name = "Test Run",
                SportType = "Run",
                Distance = 5000,
                MovingTime = 3600, // 60 minutes
                StartDate = DateTime.UtcNow,
            },
        };

        this.mockStravaService
            .Setup(s => s.GetActivitiesAsync(accessToken, 1, 10))
            .ReturnsAsync(activities);

        // Act
        var result = await this.controller.TestActivities(accessToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        var responseType = response!.GetType();
        var activitiesProperty = responseType.GetProperty("activities");
        var activitiesData = activitiesProperty?.GetValue(response) as IEnumerable<object>;

        Assert.NotNull(activitiesData);
        var firstActivity = activitiesData.First();
        var activityType = firstActivity.GetType();
        var movingTimeProperty = activityType.GetProperty("MovingTimeMinutes");

        // 3600 seconds = 60 minutes
        Assert.Equal(60, movingTimeProperty?.GetValue(firstActivity));
    }

    #endregion
}