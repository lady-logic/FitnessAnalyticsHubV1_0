using AIAssistant.Application.Interfaces;
using AIAssistant.Tests.Base;
using AIAssistant.Tests.Helpers;
using AIAssistant.UI.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace AIAssistant.Tests.Controllers;

public class DebugControllerTests : AIAssistantControllerTestBase<DebugController>
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<DebugController>> _mockLogger;
    private readonly DebugController _controller;

    public DebugControllerTests()
    {
        _mockConfiguration = MockSetup.CreateMockConfiguration();
        _mockLogger = MockSetup.CreateMockLogger<DebugController>();
        _controller = new DebugController(_mockConfiguration.Object, _mockLogger.Object);
    }

    #region ConfigCheck Tests

    [Fact]
    public async Task ConfigCheck_WithValidConfiguration_ReturnsConfigurationInfo()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns("test_api_key_1234567890");

        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.GetChildren()).Returns(new List<IConfigurationSection>());
        _mockConfiguration.Setup(c => c.GetSection("HuggingFace")).Returns(mockSection.Object);

        // Act
        var result = await _controller.ConfigCheck();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        // Use reflection to verify response structure
        var responseType = response!.GetType();
        var hasApiKeyProperty = responseType.GetProperty("hasApiKey");
        var apiKeyLengthProperty = responseType.GetProperty("apiKeyLength");
        var apiKeyPreviewProperty = responseType.GetProperty("apiKeyPreview");

        Assert.True((bool)hasApiKeyProperty!.GetValue(response)!);
        Assert.Equal(23, (int)apiKeyLengthProperty!.GetValue(response)!);
        Assert.Equal("test_api_k...", apiKeyPreviewProperty!.GetValue(response));
    }

    [Fact]
    public async Task ConfigCheck_WithMissingApiKey_ReturnsNoKeyInfo()
    {
        // Arrange
        SetupMockConfiguration(null);

        // Act
        var result = await _controller.ConfigCheck();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        var responseType = response!.GetType();
        var hasApiKeyProperty = responseType.GetProperty("hasApiKey");
        var apiKeyLengthProperty = responseType.GetProperty("apiKeyLength");
        var apiKeyPreviewProperty = responseType.GetProperty("apiKeyPreview");

        Assert.False((bool)hasApiKeyProperty!.GetValue(response)!);
        Assert.Equal(0, (int)apiKeyLengthProperty!.GetValue(response)!);
        Assert.Equal("NULL", apiKeyPreviewProperty!.GetValue(response));
    }

    private void SetupMockConfiguration(string? apiKey = null)
    {
        _mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns(apiKey);

        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.GetChildren()).Returns(new List<IConfigurationSection>());
        _mockConfiguration.Setup(c => c.GetSection("HuggingFace")).Returns(mockSection.Object);
    }
    #endregion

    #region TestModernHuggingFace Tests

    [Fact]
    public async Task TestModernHuggingFace_WithValidApiKey_ReturnsTestResults()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns("valid_test_key");

        // Act
        var result = await _controller.TestModernHuggingFace();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        var responseType = response!.GetType();
        var messageProperty = responseType.GetProperty("message");
        var testResultsProperty = responseType.GetProperty("testResults");

        Assert.Contains("Modern HuggingFace", messageProperty!.GetValue(response)!.ToString());
        Assert.NotNull(testResultsProperty!.GetValue(response));
    }

    [Fact]
    public async Task TestModernHuggingFace_WithMissingApiKey_ReturnsBadRequest()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns((string?)null);

        // Act
        var result = await _controller.TestModernHuggingFace();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No HuggingFace API key found", badRequestResult.Value);
    }

    #endregion

    #region TestHuggingFaceService Tests

    [Fact]
    public async Task TestHuggingFaceService_WithWorkingService_ReturnsSuccessInfo()
    {
        // Arrange
        var mockMotivationService = MockSetup.CreateMockMotivationCoachService();

        // Setup the service provider to return our mock
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IMotivationCoachService)))
                          .Returns(mockMotivationService.Object);

        var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        httpContextMock.Setup(ctx => ctx.RequestServices)
                      .Returns(serviceProviderMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Act
        var result = await _controller.TestHuggingFaceService();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        var responseType = response!.GetType();
        var messageProperty = responseType.GetProperty("message");
        var isSuccessProperty = responseType.GetProperty("isSuccess");

        Assert.Contains("HuggingFaceService Test", messageProperty!.GetValue(response)!.ToString());
        Assert.True((bool)isSuccessProperty!.GetValue(response)!);
    }

    #endregion

    #region DirectHuggingFaceTest Tests

    [Fact]
    public async Task DirectHuggingFaceTest_WithValidKey_ReturnsApiResponse()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns("valid_api_key");

        // Act
        var result = await _controller.DirectHuggingFaceTest();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        var responseType = response!.GetType();
        var statusCodeProperty = responseType.GetProperty("statusCode");
        var requestUrlProperty = responseType.GetProperty("requestUrl");

        Assert.NotNull(statusCodeProperty!.GetValue(response));
        Assert.Contains("huggingface.co", requestUrlProperty!.GetValue(response)!.ToString());
    }

    [Fact]
    public async Task DirectHuggingFaceTest_WithMissingKey_ReturnsBadRequest()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns((string?)null);

        // Act
        var result = await _controller.DirectHuggingFaceTest();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("No HuggingFace API key found in configuration", badRequestResult.Value);
    }

    #endregion

    #region TestWithoutAuth Tests

    [Fact]
    public async Task TestWithoutAuth_ReturnsResponseWithoutAuthentication()
    {
        // Act
        var result = await _controller.TestWithoutAuth();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        var responseType = response!.GetType();
        var messageProperty = responseType.GetProperty("message");

        Assert.Equal("Test without authentication", messageProperty!.GetValue(response));
    }

    #endregion

    #region HealthCheck Tests

    [Fact]
    public async Task HealthCheck_ReturnsHealthyStatus()
    {
        // Arrange
        _mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns("test_key");

        // Act
        var result = await _controller.HealthCheck();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        var responseType = response!.GetType();
        var statusProperty = responseType.GetProperty("status");
        var messageProperty = responseType.GetProperty("message");
        var configurationProperty = responseType.GetProperty("configuration");
        var availableTestsProperty = responseType.GetProperty("availableTests");

        Assert.Equal("healthy", statusProperty!.GetValue(response));
        Assert.Equal("Debug controller is responding", messageProperty!.GetValue(response));
        Assert.NotNull(configurationProperty!.GetValue(response));
        Assert.NotNull(availableTestsProperty!.GetValue(response));
    }

    [Fact]
    public async Task HealthCheck_WhenExceptionOccurs_ThrowsException()
    {
        // Arrange
        var mockConfigThrowsException = new Mock<IConfiguration>();
        mockConfigThrowsException.Setup(c => c["HuggingFace:ApiKey"])
                                 .Throws(new Exception("Configuration error"));
        var controllerWithFailingConfig = new DebugController(
            mockConfigThrowsException.Object,
            _mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => controllerWithFailingConfig.HealthCheck());
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
        _mockConfiguration.Setup(c => c["HuggingFace:ApiKey"]).Returns(apiKey);

        var mockSection = new Mock<IConfigurationSection>();
        mockSection.Setup(s => s.GetChildren()).Returns(new List<IConfigurationSection>());
        _mockConfiguration.Setup(c => c.GetSection("HuggingFace")).Returns(mockSection.Object);

        // Act
        var result = await _controller.ConfigCheck();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;
        var responseType = response!.GetType();
        var apiKeyLengthProperty = responseType.GetProperty("apiKeyLength");
        var hasApiKeyProperty = responseType.GetProperty("hasApiKey");

        Assert.Equal(apiKey.Length, (int)apiKeyLengthProperty!.GetValue(response)!);
        Assert.Equal(!string.IsNullOrEmpty(apiKey), (bool)hasApiKeyProperty!.GetValue(response)!);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task TestHuggingFaceService_WhenServiceThrowsException_ReturnsError()
    {
        // Arrange
        var mockMotivationService = new Mock<IMotivationCoachService>();
        mockMotivationService.Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<AIAssistant.Applications.DTOs.MotivationRequestDto>()))
                           .ThrowsAsync(new Exception("Service unavailable"));

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IMotivationCoachService)))
                          .Returns(mockMotivationService.Object);

        var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
        httpContextMock.Setup(ctx => ctx.RequestServices)
                      .Returns(serviceProviderMock.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object
        };

        // Act
        var result = await _controller.TestHuggingFaceService();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);

        var response = statusCodeResult.Value;
        var responseType = response!.GetType();
        var errorProperty = responseType.GetProperty("error");

        Assert.Contains("Service unavailable", errorProperty!.GetValue(response)!.ToString());
    }

    #endregion
}