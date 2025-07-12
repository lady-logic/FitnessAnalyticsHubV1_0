using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using AIAssistant.Infrastructure.Services;
using AIAssistant.Tests.Helpers;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.Extensions.Logging;
using Moq;

namespace AIAssistant.Tests.Infrastructure.Services;

public class MotivationCoachServiceTests
{
    private readonly Mock<IAIPromptService> _mockAIPromptService;
    private readonly Mock<ILogger<MotivationCoachService>> _mockLogger;
    private readonly MotivationCoachService _service;

    public MotivationCoachServiceTests()
    {
        _mockAIPromptService = new Mock<IAIPromptService>();
        _mockLogger = MockSetup.CreateMockLogger<MotivationCoachService>();
        _service = new MotivationCoachService(_mockAIPromptService.Object, _mockLogger.Object);
    }

    #region GenerateMotivationAsync Tests

    [Fact]
    public async Task GenerateMotivationAsync_WithValidRequest_ReturnsMotivationalResponse()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var aiResponse = CreateWellFormattedAIResponse();

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.MotivationalMessage));
        Assert.NotNull(result.Quote);
        Assert.NotNull(result.ActionableTips);
        Assert.True(result.ActionableTips.Count > 0);
        Assert.True(result.GeneratedAt > DateTime.MinValue);

        MockSetup.VerifyLoggerCalledWithInformation(_mockLogger, "Generating motivational message", Times.Once());
        MockSetup.VerifyLoggerCalledWithInformation(_mockLogger, "Successfully generated motivational message", Times.Once());
    }

    [Fact]
    public async Task GenerateMotivationAsync_WithStructuredAIResponse_ParsesCorrectly()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var structuredResponse = @"
Great job on your fitness journey! You're making excellent progress and your dedication is truly inspiring.

Quote: ""Success is not final, failure is not fatal: it is the courage to continue that counts.""

Tips:
1. Set realistic daily goals that challenge but don't overwhelm you
2. Track your progress weekly to see how far you've come
3. Celebrate small wins along the way to maintain motivation
";

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(structuredResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Contains("Great job on your fitness journey", result.MotivationalMessage);
        Assert.Contains("Success is not final", result.Quote);
        Assert.Equal(3, result.ActionableTips!.Count);
        Assert.Contains("Set realistic daily goals", result.ActionableTips[0]);
    }

    [Fact]
    public async Task GenerateMotivationAsync_WithMinimalAIResponse_HandlesGracefully()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var minimalResponse = "Keep pushing forward! You've got this!";

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(minimalResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Equal(minimalResponse, result.MotivationalMessage);
        Assert.Null(result.Quote); // No quote found
        Assert.Null(result.ActionableTips); // No tips found
    }

    [Fact]
    public async Task GenerateMotivationAsync_WhenAIServiceThrowsException_ReturnsFallbackResponse()
    {
        // Arrange
        var request = CreateTestMotivationRequest("John");

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("AI service unavailable"));

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("John", result.MotivationalMessage); // Fallback uses athlete name
        Assert.Contains("Robert Collier", result.Quote!); // Default fallback quote
        Assert.Equal(3, result.ActionableTips!.Count);
        Assert.Contains("Set small, achievable goals", result.ActionableTips[0]);

        MockSetup.VerifyLoggerCalledWithError(_mockLogger, "Error generating motivational message", Times.Once());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateMotivationAsync_WithEmptyAIResponse_ReturnsDefaultMessage(string emptyResponse)
    {
        // Arrange
        var request = CreateTestMotivationRequest();

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(emptyResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Equal("You're doing great! Keep up the excellent work with your fitness journey.", result.MotivationalMessage);
        Assert.Null(result.Quote);
        Assert.Null(result.ActionableTips);
    }

    #endregion

    #region GetHuggingFaceMotivationalMessageAsync Tests

    [Fact]
    public async Task GetHuggingFaceMotivationalMessageAsync_CallsGenerateMotivationAsync()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var aiResponse = "You're doing amazing! Keep it up!";

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GetHuggingFaceMotivationalMessageAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(aiResponse, result.MotivationalMessage);

        _mockAIPromptService.Verify(s => s.GetMotivationAsync(It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Prompt Building Tests

    [Fact]
    public async Task GenerateMotivationAsync_BuildsPromptWithAthleteInfo()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "Sarah",
                FitnessLevel = "Advanced",
                PrimaryGoal = "Marathon Training"
            },
            IsStruggling = true
        };

        string capturedPrompt = "";
        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .Callback<string>(prompt => capturedPrompt = prompt)
            .ReturnsAsync("Great job!");

        // Act
        await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Contains("Sarah", capturedPrompt);
        Assert.Contains("Advanced", capturedPrompt);
        Assert.Contains("Marathon Training", capturedPrompt);
        Assert.Contains("struggling with motivation", capturedPrompt);
    }

    [Fact]
    public async Task GenerateMotivationAsync_WithLastWorkout_IncludesWorkoutInfo()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = CreateTestAthleteProfile(),
            LastWorkout = new WorkoutDataDto
            {
                ActivityType = "Run",
                Distance = 10,
                Duration = 3600, // 1 hour
                Date = DateTime.Now.AddDays(-1)
            }
        };

        string capturedPrompt = "";
        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .Callback<string>(prompt => capturedPrompt = prompt)
            .ReturnsAsync("Great run!");

        // Act
        await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Contains("Last workout: Run", capturedPrompt);
        Assert.Contains("10km", capturedPrompt);
        Assert.Contains("01:00:00", capturedPrompt); // Duration formatted
    }

    [Fact]
    public async Task GenerateMotivationAsync_WithNullAthleteProfile_UsesDefaults()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = null,
            IsStruggling = false
        };

        string capturedPrompt = "";
        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .Callback<string>(prompt => capturedPrompt = prompt)
            .ReturnsAsync("Keep going!");

        // Act
        await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Contains("Champion", capturedPrompt); // Default name
        Assert.Contains("Beginner", capturedPrompt); // Default fitness level
        Assert.Contains("General Fitness", capturedPrompt); // Default goal
    }

    #endregion

    #region Parsing Tests

    [Fact]
    public async Task ExtractQuote_WithQuoteLabel_ExtractsCorrectly()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var aiResponse = @"
Great work on your fitness journey!

Quote: Success is not about being perfect, it's about being better than yesterday.

Keep pushing forward!";

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Equal("Success is not about being perfect, it's about being better than yesterday.", result.Quote);
    }

    [Fact]
    public async Task ExtractTips_WithNumberedList_ExtractsCorrectly()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var aiResponse = @"
Keep up the great work!

Tips:
1. Set small daily goals that are achievable
2. Track your progress to see improvements
3. Reward yourself for consistency
4. This tip should be ignored as we limit to 3
";

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.NotNull(result.ActionableTips);
        Assert.Equal(3, result.ActionableTips.Count); // Limited to 3
        Assert.Contains("Set small daily goals", result.ActionableTips[0]);
        Assert.Contains("Track your progress", result.ActionableTips[1]);
        Assert.Contains("Reward yourself", result.ActionableTips[2]);
    }

    [Fact]
    public async Task ExtractTips_WithBulletPoints_ExtractsCorrectly()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var aiResponse = @"
Excellent progress!

Actionable tips:
• Focus on consistency over perfection
- Celebrate small victories daily
* Remember why you started this journey
";

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.NotNull(result.ActionableTips);
        Assert.Equal(3, result.ActionableTips.Count);
        Assert.Contains("Focus on consistency", result.ActionableTips[0]);
        Assert.Contains("Celebrate small victories", result.ActionableTips[1]);
        Assert.Contains("Remember why you started", result.ActionableTips[2]);
    }

    [Fact]
    public async Task ExtractMotivationalMessage_WithLongResponse_LimitsLength()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var longMessage = string.Join(". ", Enumerable.Repeat("This is a very long motivational sentence that goes on and on", 10));
        var aiResponse = $"{longMessage}. Quote: \"Test quote\"";

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.True(result.MotivationalMessage.Length <= 300); // Should be limited
        Assert.EndsWith(".", result.MotivationalMessage); // Should end properly
    }

    #endregion

    #region Fallback Tests

    [Fact]
    public async Task GetFallbackMotivation_GeneratesRandomMessages()
    {
        // Arrange
        var request = CreateTestMotivationRequest("TestAthlete");
        var messages = new HashSet<string>();

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Service down"));

        // Act - Generate multiple fallback messages
        for (int i = 0; i < 10; i++)
        {
            var result = await _service.GenerateMotivationAsync(request);
            messages.Add(result.MotivationalMessage);
        }

        // Assert - Should generate different messages due to randomization
        Assert.True(messages.Count > 1, "Should generate different fallback messages");
        Assert.All(messages, msg => Assert.Contains("TestAthlete", msg));
    }

    [Fact]
    public async Task FallbackResponse_HasDefaultQuoteAndTips()
    {
        // Arrange
        var request = CreateTestMotivationRequest();

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Contains("Robert Collier", result.Quote!);
        Assert.Equal(3, result.ActionableTips!.Count);
        Assert.Contains("Set small, achievable goals", result.ActionableTips[0]);
        Assert.Contains("Focus on consistency", result.ActionableTips[1]);
        Assert.Contains("Celebrate every small victory", result.ActionableTips[2]);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GenerateMotivationAsync_WithSpecialCharactersInName_HandlesCorrectly()
    {
        // Arrange
        var request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "María José-Smith",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "General Fitness"
            }
        };

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync("¡Excelente trabajo, María!");

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Contains("María", result.MotivationalMessage);
    }

    [Fact]
    public async Task ExtractTips_WithVeryShortTips_FiltersOut()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var aiResponse = @"
Tips:
1. Go! 
2. This tip is long enough to be included in the results
3. Yes
4. Another good tip that meets the minimum length requirement
";

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.NotNull(result.ActionableTips);
        Assert.Equal(2, result.ActionableTips.Count); // Only the long enough tips
        Assert.All(result.ActionableTips, tip => Assert.True(tip.Length > 15));
    }

    [Fact]
    public async Task ExtractQuote_WithTechnicalContent_FiltersOut()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var aiResponse = @"
Great job!
""HTTP 404 error occurred while processing the request""
""Believe in yourself and achieve your dreams""
Keep going!";

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(aiResponse);

        // Act
        var result = await _service.GenerateMotivationAsync(request);

        // Assert
        Assert.Equal("Believe in yourself and achieve your dreams", result.Quote);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GenerateMotivationAsync_WithVeryLargeAIResponse_HandlesEfficiently()
    {
        // Arrange
        var request = CreateTestMotivationRequest();
        var largeResponse = string.Join("\n", Enumerable.Repeat("This is a line of AI response text.", 1000));

        _mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>()))
            .ReturnsAsync(largeResponse);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _service.GenerateMotivationAsync(request);
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000); // Should process quickly
        Assert.NotNull(result);
        Assert.True(result.MotivationalMessage.Length <= 300); // Should be limited
    }

    #endregion

    #region Helper Methods

    private static MotivationRequestDto CreateTestMotivationRequest(string athleteName = "Test User")
    {
        return new MotivationRequestDto
        {
            AthleteProfile = CreateTestAthleteProfile(athleteName),
            IsStruggling = false,
            LastWorkout = null,
            UpcomingWorkoutType = null
        };
    }

    private static AthleteProfileDto CreateTestAthleteProfile(string name = "Test User")
    {
        return new AthleteProfileDto
        {
            Id = "123",
            Name = name,
            FitnessLevel = "Intermediate",
            PrimaryGoal = "General Fitness"
        };
    }

    private static string CreateWellFormattedAIResponse()
    {
        return @"Fantastic work on your fitness journey! Your dedication and consistency are truly inspiring, and every step you take brings you closer to your goals.

Quote: ""Success is not about being perfect, it's about being better than yesterday.""

Tips:
1. Set realistic daily goals that challenge but don't overwhelm you
2. Track your progress weekly to celebrate your improvements
3. Remember to rest and recover - growth happens during recovery";
    }

    #endregion
}