namespace AIAssistant.Tests.Controllers;

using System.Net;
using AIAssistant.Application.Interfaces;
using AIAssistant.Tests.Base;
using AIAssistant.Tests.Helpers;
using AIAssistant.UI.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

public class DebugControllerTests : AIAssistantControllerTestBase<DebugController>
{
    private readonly Mock<IConfiguration> mockConfiguration;
    private readonly Mock<ILogger<DebugController>> mockLogger;
    private readonly DebugController controller;

    public DebugControllerTests()
    {
        this.mockConfiguration = MockSetup.CreateMockConfiguration();
        this.mockLogger = MockSetup.CreateMockLogger<DebugController>();
        this.controller = new DebugController(this.mockConfiguration.Object, this.mockLogger.Object);
    }

    #region ConfigCheck Tests

    [Fact]
    public async Task ConfigCheck_WithValidConfiguration_ReturnsConfigurationInfo()
    {
        // Arrange
        this.mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns("test_api_key_1234567890");

        Mock<IConfigurationSection> mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.GetChildren()).Returns(new List<IConfigurationSection>());
        this.mockConfiguration.Setup(c => c.GetSection("HuggingFace")).Returns(mockSection.Object);

        // Act
        ActionResult result = await this.controller.ConfigCheck();

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;

        // Use reflection to verify response structure
        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? hasApiKeyProperty = responseType.GetProperty("hasApiKey");
        System.Reflection.PropertyInfo? apiKeyLengthProperty = responseType.GetProperty("apiKeyLength");
        System.Reflection.PropertyInfo? apiKeyPreviewProperty = responseType.GetProperty("apiKeyPreview");

        Assert.True((bool)hasApiKeyProperty!.GetValue(response)!);
        Assert.Equal(23, (int)apiKeyLengthProperty!.GetValue(response)!);
        Assert.Equal("test_api_k...", apiKeyPreviewProperty!.GetValue(response));
    }

    [Fact]
    public async Task ConfigCheck_WithMissingApiKey_ReturnsNoKeyInfo()
    {
        // Arrange
        this.SetupMockConfiguration(null);

        // Act
        ActionResult result = await this.controller.ConfigCheck();

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;

        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? hasApiKeyProperty = responseType.GetProperty("hasApiKey");
        System.Reflection.PropertyInfo? apiKeyLengthProperty = responseType.GetProperty("apiKeyLength");
        System.Reflection.PropertyInfo? apiKeyPreviewProperty = responseType.GetProperty("apiKeyPreview");

        Assert.False((bool)hasApiKeyProperty!.GetValue(response)!);
        Assert.Equal(0, (int)apiKeyLengthProperty!.GetValue(response)!);
        Assert.Equal("NULL", apiKeyPreviewProperty!.GetValue(response));
    }

    private void SetupMockConfiguration(string? apiKey = null)
    {
        this.mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns(apiKey);

        Mock<IConfigurationSection> mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.GetChildren()).Returns(new List<IConfigurationSection>());
        this.mockConfiguration.Setup(c => c.GetSection("HuggingFace")).Returns(mockSection.Object);
    }
    #endregion

    #region TestModernHuggingFace Tests

    [Fact]
    public async Task TestModernHuggingFace_WithValidApiKey_ReturnsTestResults()
    {
        // Arrange
        this.mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns("valid_test_key");

        // Act
        ActionResult result = await this.controller.TestModernHuggingFace(CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;

        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? messageProperty = responseType.GetProperty("message");
        System.Reflection.PropertyInfo? testResultsProperty = responseType.GetProperty("testResults");

        Assert.Contains("Modern HuggingFace", messageProperty!.GetValue(response)!.ToString());
        Assert.NotNull(testResultsProperty!.GetValue(response));
    }

    [Fact]
    public async Task TestModernHuggingFace_WithMissingApiKey_ReturnsBadRequest()
    {
        // Arrange
        this.mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns((string?)null);

        // Act
        ActionResult result = await this.controller.TestModernHuggingFace(CancellationToken.None);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No HuggingFace API key found", badRequestResult.Value);
    }

    #endregion

    #region TestHuggingFaceService Tests

    [Fact]
    public async Task TestHuggingFaceService_WithWorkingService_ReturnsSuccessInfo()
    {
        // Arrange
        Mock<IMotivationCoachService> mockMotivationService = MockSetup.CreateMockMotivationCoachService();

        // Setup the service provider to return our mock
        Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IMotivationCoachService)))
                          .Returns(mockMotivationService.Object);

        Mock<Microsoft.AspNetCore.Http.HttpContext> httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        httpContextMock.Setup(ctx => ctx.RequestServices)
                      .Returns(serviceProviderMock.Object);

        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object,
        };

        // Act
        ActionResult result = await this.controller.TestHuggingFaceService(CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;

        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? messageProperty = responseType.GetProperty("message");
        System.Reflection.PropertyInfo? isSuccessProperty = responseType.GetProperty("isSuccess");

        Assert.Contains("HuggingFaceService Test", messageProperty!.GetValue(response)!.ToString());
        Assert.True((bool)isSuccessProperty!.GetValue(response)!);
    }

    #endregion

    #region DirectHuggingFaceTest Tests

    [Fact]
    public async Task DirectHuggingFaceTest_WithValidKey_ReturnsApiResponse()
    {
        // Arrange
        this.mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns("valid_api_key");

        // Act
        ActionResult result = await this.controller.DirectHuggingFaceTest(CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;

        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? statusCodeProperty = responseType.GetProperty("statusCode");
        System.Reflection.PropertyInfo? requestUrlProperty = responseType.GetProperty("requestUrl");

        Assert.NotNull(statusCodeProperty!.GetValue(response));
        Assert.Contains("huggingface.co", requestUrlProperty!.GetValue(response)!.ToString());
    }

    [Fact]
    public async Task DirectHuggingFaceTest_WithMissingKey_ReturnsBadRequest()
    {
        // Arrange
        this.mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns((string?)null);

        // Act
        ActionResult result = await this.controller.DirectHuggingFaceTest(CancellationToken.None);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No HuggingFace API key found in configuration", badRequestResult.Value);
    }

    #endregion

    #region TestWithoutAuth Tests

    [Fact]
    public async Task TestWithoutAuth_ReturnsResponseWithoutAuthentication()
    {
        // Act
        ActionResult result = await this.controller.TestWithoutAuth(CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;

        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? messageProperty = responseType.GetProperty("message");

        Assert.Equal("Test without authentication", messageProperty!.GetValue(response));
    }

    #endregion

    #region HealthCheck Tests

    [Fact]
    public async Task HealthCheck_ReturnsHealthyStatus()
    {
        // Arrange
        this.mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns("test_key");

        // Act
        ActionResult result = await this.controller.HealthCheck();

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;

        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? statusProperty = responseType.GetProperty("status");
        System.Reflection.PropertyInfo? messageProperty = responseType.GetProperty("message");
        System.Reflection.PropertyInfo? configurationProperty = responseType.GetProperty("configuration");
        System.Reflection.PropertyInfo? availableTestsProperty = responseType.GetProperty("availableTests");

        Assert.Equal("healthy", statusProperty!.GetValue(response));
        Assert.Equal("Debug controller is responding", messageProperty!.GetValue(response));
        Assert.NotNull(configurationProperty!.GetValue(response));
        Assert.NotNull(availableTestsProperty!.GetValue(response));
    }

    [Fact]
    public async Task HealthCheck_WhenExceptionOccurs_ThrowsException()
    {
        // Arrange
        Mock<IConfiguration> mockConfigThrowsException = new Mock<IConfiguration>();
        mockConfigThrowsException.Setup(c => c["HuggingFace:ApiKey"])
                                 .Throws(new Exception("Configuration error"));
        DebugController controllerWithFailingConfig = new DebugController(
            mockConfigThrowsException.Object,
            this.mockLogger.Object);

        // Act & Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(() => controllerWithFailingConfig.HealthCheck());
        Assert.Equal("Configuration error", exception.Message);
    }

    #endregion

    #region Integration Tests

    [Theory]
    [InlineData("test_key_short")]
    [InlineData("very_long_api_key_that_exceeds_normal_length_expectations_for_testing_purposes")]
    public async Task ConfigCheck_WithDifferentKeyLengths_ReturnsCorrectInfo(string apiKey)
    {
        // Arrange
        this.mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns(apiKey);

        Mock<IConfigurationSection> mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.GetChildren()).Returns(new List<IConfigurationSection>());
        this.mockConfiguration.Setup(c => c.GetSection("HuggingFace")).Returns(mockSection.Object);

        // Act
        ActionResult result = await this.controller.ConfigCheck();

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        object? response = okResult.Value;
        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? apiKeyLengthProperty = responseType.GetProperty("apiKeyLength");
        System.Reflection.PropertyInfo? hasApiKeyProperty = responseType.GetProperty("hasApiKey");

        Assert.Equal(apiKey.Length, (int)apiKeyLengthProperty!.GetValue(response)!);
        Assert.Equal(!string.IsNullOrEmpty(apiKey), (bool)hasApiKeyProperty!.GetValue(response)!);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task TestHuggingFaceService_WhenServiceThrowsException_ReturnsError()
    {
        // Arrange
        Mock<IMotivationCoachService> mockMotivationService = new Mock<IMotivationCoachService>();
        mockMotivationService.Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<AIAssistant.Applications.DTOs.MotivationRequestDto>(), CancellationToken.None))
                           .ThrowsAsync(new Exception("Service unavailable"));

        Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IMotivationCoachService)))
                          .Returns(mockMotivationService.Object);

        Mock<Microsoft.AspNetCore.Http.HttpContext> httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        httpContextMock.Setup(ctx => ctx.RequestServices)
                      .Returns(serviceProviderMock.Object);

        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object,
        };

        // Act
        ActionResult result = await this.controller.TestHuggingFaceService(CancellationToken.None);

        // Assert
        ObjectResult statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);

        object? response = statusCodeResult.Value;
        Type responseType = response!.GetType();
        System.Reflection.PropertyInfo? errorProperty = responseType.GetProperty("error");

        Assert.Contains("Service unavailable", errorProperty!.GetValue(response)!.ToString());
    }

    #endregion
}