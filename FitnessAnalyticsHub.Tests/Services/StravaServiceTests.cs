namespace FitnessAnalyticsHub.Tests.Services
{
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using FitnessAnalyticsHub.Infrastructure.Configuration;
    using FitnessAnalyticsHub.Infrastructure.Exceptions;
    using FitnessAnalyticsHub.Infrastructure.Services;
    using Microsoft.Extensions.Options;
    using Moq;
    using Moq.Protected;

    public class StravaServiceTests : IDisposable
    {
        private readonly Mock<IHttpClientFactory> mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> mockHttpMessageHandler;
        private readonly HttpClient httpClient;
        private readonly Mock<IOptions<StravaConfiguration>> mockOptions;
        private readonly StravaConfiguration config;
        private readonly StravaService stravaService;

        public StravaServiceTests()
        {
            this.mockHttpClientFactory = new Mock<IHttpClientFactory>();
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(this.mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://www.strava.com/api/v3/"),
            };

            this.config = new StravaConfiguration
            {
                ClientId = "test_client_id",
                ClientSecret = "test_client_secret",
                RedirectUrl = "http://localhost/callback",
                BaseUrl = "https://www.strava.com/api/v3/",
                AuthorizeUrl = "https://www.strava.com/oauth/authorize",
                TokenUrl = "https://www.strava.com/oauth/token",
            };

            this.mockOptions = new Mock<IOptions<StravaConfiguration>>();
            this.mockOptions.Setup(x => x.Value).Returns(this.config);

            this.mockHttpClientFactory.Setup(x => x.CreateClient("StravaApi"))
                                  .Returns(this.httpClient);

            this.stravaService = new StravaService(this.mockHttpClientFactory.Object, this.mockOptions.Object);
        }

        #region GetAuthorizationUrlAsync Tests

        [Fact]
        public async Task GetAuthorizationUrlAsync_ShouldReturnCorrectUrl()
        {
            // Act
            string result = await this.stravaService.GetAuthorizationUrlAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("client_id=test_client_id", result);
            Assert.Contains("response_type=code", result);
            Assert.Contains("scope=read_all,activity:read_all", result);
            Assert.Contains("approval_prompt=force", result);
            Assert.Contains("redirect_uri=", result);
        }

        #endregion

        #region ExchangeCodeForTokenAsync Tests

        [Fact]
        public async Task ExchangeCodeForTokenAsync_WithValidCode_ShouldReturnTokenInfo()
        {
            // Arrange
            string authCode = "test_auth_code";
            var tokenResponse = new
            {
                token_type = "Bearer",
                access_token = "test_access_token",
                expires_at = 1234567890,
                expires_in = 3600,
                refresh_token = "test_refresh_token",
            };

            string jsonResponse = JsonSerializer.Serialize(tokenResponse);
            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
            };

            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            Domain.Models.TokenInfo result = await this.stravaService.ExchangeCodeForTokenAsync(authCode, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Bearer", result.TokenType);
            Assert.Equal("test_access_token", result.AccessToken);
            Assert.Equal(1234567890, result.ExpiresAt);
            Assert.Equal(3600, result.ExpiresIn);
            Assert.Equal("test_refresh_token", result.RefreshToken);
        }

        [Fact]
        public Task ExchangeCodeForTokenAsync_WithHttpError_ShouldThrowException()
        {
            // Arrange
            string authCode = "invalid_code";
            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act & Assert
            return Assert.ThrowsAsync<HttpRequestException>(
                () => this.stravaService.ExchangeCodeForTokenAsync(authCode, CancellationToken.None));
        }

        #endregion

        #region GetAthleteProfileAsync Tests

        [Fact]
        public async Task GetAthleteProfileAsync_WithValidToken_ShouldReturnAthlete()
        {
            // Arrange
            string accessToken = "valid_token";
            var athleteResponse = new
            {
                id = 12345L,
                username = "testuser",
                firstname = "Test",
                lastname = "User",
                bio = "Test bio",
                city = "Test City",
                country = "Test Country",
                profile = "https://example.com/profile.jpg",
                email = "test@example.com",
            };

            string jsonResponse = JsonSerializer.Serialize(athleteResponse);
            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
            };

            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains("athlete") &&
                        req.Headers.Authorization.Parameter == accessToken),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            Domain.Entities.Athlete result = await this.stravaService.GetAthleteProfileAsync(accessToken, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("12345", result.StravaId);
            Assert.Equal("Test", result.FirstName);
            Assert.Equal("User", result.LastName);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
            Assert.Equal("Test City", result.City);
            Assert.Equal("Test Country", result.Country);
        }

        [Fact]
        public async Task GetAthleteProfileAsync_WithNullToken_ShouldThrowInvalidStravaTokenException()
        {
            // Act & Assert
            InvalidStravaTokenException exception = await Assert.ThrowsAsync<InvalidStravaTokenException>(
                () => this.stravaService.GetAthleteProfileAsync(null, CancellationToken.None));

            Assert.Equal("Access token cannot be null or empty", exception.Message);
        }

        [Fact]
        public async Task GetAthleteProfileAsync_WithEmptyToken_ShouldThrowInvalidStravaTokenException()
        {
            // Act & Assert
            InvalidStravaTokenException exception = await Assert.ThrowsAsync<InvalidStravaTokenException>(
                () => this.stravaService.GetAthleteProfileAsync("   ", CancellationToken.None));

            Assert.Equal("Access token cannot be null or empty", exception.Message);
        }

        #endregion

        #region GetActivitiesAsync Tests

        [Fact]
        public async Task GetActivitiesAsync_WithValidToken_ShouldReturnActivities()
        {
            // Arrange
            string accessToken = "valid_token";
            var activitiesResponse = new[]
            {
                new
                {
                    id = 111L,
                    name = "Morning Run",
                    distance = 5000f,
                    moving_time = 1800,
                    elapsed_time = 1900,
                    total_elevation_gain = 100f,
                    sport_type = "Run",
                    start_date = DateTime.UtcNow.AddDays(-1),
                    start_date_local = DateTime.Now.AddDays(-1),
                    timezone = "Europe/Berlin",
                    average_speed = 2.78f,
                    max_speed = 4.17f,
                    average_heartrate = (int?)150,
                    max_heartrate = (int?)180,
                    average_watts = (double?)250,
                    max_watts = (double?)400,
                    average_cadence = (double?)90,
                },
                new
                {
                    id = 222L,
                    name = "Evening Ride",
                    distance = 20000f,
                    moving_time = 3600,
                    elapsed_time = 3700,
                    total_elevation_gain = 300f,
                    sport_type = "Ride",
                    start_date = DateTime.UtcNow.AddDays(-2),
                    start_date_local = DateTime.Now.AddDays(-2),
                    timezone = "Europe/Berlin",
                    average_speed = 5.56f,
                    max_speed = 8.33f,
                    average_heartrate = (int?)140,
                    max_heartrate = (int?)170,
                    average_watts = (double?)200,
                    max_watts = (double?)350,
                    average_cadence = (double?)85,
                },
            };

            string jsonResponse = JsonSerializer.Serialize(activitiesResponse);
            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
            };

            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri.ToString().Contains("athlete/activities") &&
                        req.Headers.Authorization.Parameter == accessToken),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act
            IEnumerable<Domain.Entities.Activity> result = await this.stravaService.GetActivitiesAsync(accessToken, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            Domain.Entities.Activity firstActivity = result.First();
            Assert.Equal("111", firstActivity.StravaId);
            Assert.Equal("Morning Run", firstActivity.Name);
            Assert.Equal(5000, firstActivity.Distance);
            Assert.Equal("Run", firstActivity.SportType);
        }

        [Fact]
        public async Task GetActivitiesAsync_WithUnauthorizedResponse_ShouldThrowInvalidStravaTokenException()
        {
            // Arrange
            string accessToken = "invalid_token";
            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act & Assert
            InvalidStravaTokenException exception = await Assert.ThrowsAsync<InvalidStravaTokenException>(
                () => this.stravaService.GetActivitiesAsync(accessToken, CancellationToken.None));

            Assert.Equal("Access token is invalid or expired", exception.Message);
        }

        [Fact]
        public async Task GetActivitiesAsync_WithForbiddenResponse_ShouldThrowStravaApiException()
        {
            // Arrange
            string accessToken = "valid_token";
            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.Forbidden);

            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act & Assert
            StravaApiException exception = await Assert.ThrowsAsync<StravaApiException>(
                () => this.stravaService.GetActivitiesAsync(accessToken, CancellationToken.None));

            Assert.Equal("Access forbidden - insufficient permissions", exception.Message);
            Assert.Equal(403, exception.StatusCode);
        }

        [Fact]
        public async Task GetActivitiesAsync_WithRateLimitResponse_ShouldThrowStravaApiException()
        {
            // Arrange
            string accessToken = "valid_token";
            HttpResponseMessage httpResponse = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

            this.mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            // Act & Assert
            StravaApiException exception = await Assert.ThrowsAsync<StravaApiException>(
                () => this.stravaService.GetActivitiesAsync(accessToken, CancellationToken.None));

            Assert.Equal("Rate limit exceeded - too many requests", exception.Message);
            Assert.Equal(429, exception.StatusCode);
        }

        [Fact]
        public async Task GetActivitiesAsync_WithNullToken_ShouldThrowInvalidStravaTokenException()
        {
            // Act & Assert
            InvalidStravaTokenException exception = await Assert.ThrowsAsync<InvalidStravaTokenException>(
                () => this.stravaService.GetActivitiesAsync(null, CancellationToken.None));

            Assert.Equal("Access token cannot be null or empty", exception.Message);
        }

        #endregion

        #region GenerateFreshTokenAsync Tests (indirekt über ImportMyActivitiesAsync)

        [Fact]
        public async Task ImportMyActivitiesAsync_WithMissingConfiguration_ShouldThrowStravaConfigurationException()
        {
            // Arrange
            StravaConfiguration invalidConfig = new StravaConfiguration
            {
                ClientId = string.Empty, // Leer
                ClientSecret = "test_secret",
            };

            Mock<IOptions<StravaConfiguration>> mockInvalidOptions = new Mock<IOptions<StravaConfiguration>>();
            mockInvalidOptions.Setup(x => x.Value).Returns(invalidConfig);

            StravaService invalidStravaService = new StravaService(this.mockHttpClientFactory.Object, mockInvalidOptions.Object);

            // Act & Assert
            StravaConfigurationException exception = await Assert.ThrowsAsync<StravaConfigurationException>(
                () => invalidStravaService.ImportMyActivitiesAsync(CancellationToken.None));

            Assert.Contains("ClientId and ClientSecret must be configured", exception.Message);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.httpClient?.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}