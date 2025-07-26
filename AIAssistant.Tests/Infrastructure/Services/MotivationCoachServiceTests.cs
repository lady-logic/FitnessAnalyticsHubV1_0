namespace AIAssistant.Tests.Infrastructure.Services;

using AIAssistant.Application.DTOs;
using AIAssistant.Application.Interfaces;
using AIAssistant.Applications.DTOs;
using AIAssistant.Infrastructure.Services;
using AIAssistant.Tests.Helpers;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
using Microsoft.Extensions.Logging;
using Moq;

public class MotivationCoachServiceTests
{
    private readonly Mock<IAIPromptService> mockAIPromptService;
    private readonly Mock<ILogger<MotivationCoachService>> mockLogger;
    private readonly MotivationCoachService service;

    public MotivationCoachServiceTests()
    {
        this.mockAIPromptService = new Mock<IAIPromptService>();
        this.mockLogger = MockSetup.CreateMockLogger<MotivationCoachService>();
        this.service = new MotivationCoachService(this.mockAIPromptService.Object, this.mockLogger.Object);
    }

    #region GenerateMotivationAsync Tests

    [Fact]
    public async Task GenerateMotivationAsync_WithValidRequest_ReturnsMotivationalResponse()
    {
        // Arrange
        MotivationRequestDto request = CreateTestMotivationRequest();
        string aiResponse = CreateWellFormattedAIResponse();

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(aiResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.MotivationalMessage));
        Assert.NotNull(result.Quote);
        Assert.NotNull(result.ActionableTips);
        Assert.True(result.ActionableTips.Count > 0);
        Assert.True(result.GeneratedAt > DateTime.MinValue);

        MockSetup.VerifyLoggerCalledWithInformation(this.mockLogger, "Generating motivational message", Times.Once());
        MockSetup.VerifyLoggerCalledWithInformation(this.mockLogger, "Successfully generated motivational message", Times.Once());
    }

    [Fact]
    public async Task GenerateMotivationAsync_WithStructuredAIResponse_ParsesCorrectly()
    {
        // Arrange
        MotivationRequestDto request = CreateTestMotivationRequest();
        string structuredResponse = @"
Great job on your fitness journey! You're making excellent progress and your dedication is truly inspiring.

Quote: ""Success is not final, failure is not fatal: it is the courage to continue that counts.""

Tips:
1. Set realistic daily goals that challenge but don't overwhelm you
2. Track your progress weekly to see how far you've come
3. Celebrate small wins along the way to maintain motivation
";

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(structuredResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

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
        MotivationRequestDto request = CreateTestMotivationRequest();
        string minimalResponse = "Keep pushing forward! You've got this!";

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(minimalResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(minimalResponse, result.MotivationalMessage);
        Assert.Null(result.Quote); // No quote found
        Assert.Null(result.ActionableTips); // No tips found
    }

    [Fact]
    public async Task GenerateMotivationAsync_WhenAIServiceThrowsException_ReturnsFallbackResponse()
    {
        // Arrange
        MotivationRequestDto request = CreateTestMotivationRequest("John");

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ThrowsAsync(new Exception("AI service unavailable"));

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("John", result.MotivationalMessage); // Fallback uses athlete name
        Assert.Contains("Robert Collier", result.Quote!); // Default fallback quote
        Assert.Equal(3, result.ActionableTips!.Count);
        Assert.Contains("Set small, achievable goals", result.ActionableTips[0]);

        MockSetup.VerifyLoggerCalledWithError(this.mockLogger, "Error generating motivational message", Times.Once());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateMotivationAsync_WithEmptyAIResponse_ReturnsDefaultMessage(string emptyResponse)
    {
        // Arrange
        MotivationRequestDto request = CreateTestMotivationRequest();

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(emptyResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

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
        MotivationRequestDto request = CreateTestMotivationRequest();
        string aiResponse = "You're doing amazing! Keep it up!";

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(aiResponse);

        // Act
        MotivationResponseDto result = await this.service.GetHuggingFaceMotivationalMessageAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(aiResponse, result.MotivationalMessage);

        this.mockAIPromptService.Verify(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None), Times.Once);
    }

    #endregion

    #region Prompt Building Tests

    [Fact]
    public async Task GenerateMotivationAsync_BuildsPromptWithAthleteInfo()
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "Sarah",
                FitnessLevel = "Advanced",
                PrimaryGoal = "Marathon Training",
            },
            IsStruggling = true,
        };

        string capturedPrompt = string.Empty;
        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => { /* Der Prompt wird in der Verify erfasst */ })
            .ReturnsAsync("Great job!");

        // Act
        await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert
        this.mockAIPromptService.Verify(
            s => s.GetMotivationAsync(
                It.Is<string>(prompt =>
                    prompt.Contains("Sarah") &&
                    prompt.Contains("Advanced") &&
                    prompt.Contains("Marathon Training") &&
                    prompt.Contains("struggling with motivation")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateMotivationAsync_WithLastWorkout_IncludesWorkoutInfo()
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = CreateTestAthleteProfile(),
            LastWorkout = new WorkoutDataDto
            {
                ActivityType = "Run",
                Distance = 10,
                Duration = 3600,
                Date = DateTime.Now.AddDays(-1),
            },
        };

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Great run!");

        // Act
        await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert - Direkt über Verify prüfen
        this.mockAIPromptService.Verify(
            s => s.GetMotivationAsync(
                It.Is<string>(prompt =>
                    prompt.Contains("Last workout: Run") &&
                    prompt.Contains("10km") &&
                    prompt.Contains("01:00:00")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GenerateMotivationAsync_WithNullAthleteProfile_UsesDefaults()
    {
        // Arrange
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = null,
            IsStruggling = false,
        };

        string capturedPrompt = string.Empty;
        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((prompt, token) => capturedPrompt = prompt)
            .ReturnsAsync("Keep going!");

        // Act
        await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.Contains("Champion", capturedPrompt);
        Assert.Contains("Beginner", capturedPrompt);
        Assert.Contains("General Fitness", capturedPrompt);
    }

    #endregion

    #region Parsing Tests

    [Fact]
    public async Task ExtractQuote_WithQuoteLabel_ExtractsCorrectly()
    {
        // Arrange
        MotivationRequestDto request = CreateTestMotivationRequest();
        string aiResponse = @"
Great work on your fitness journey!

Quote: Success is not about being perfect, it's about being better than yesterday.

Keep pushing forward!";

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(aiResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("Success is not about being perfect, it's about being better than yesterday.", result.Quote);
    }

    [Fact]
    public async Task ExtractTips_WithNumberedList_ExtractsCorrectly()
    {
        // Arrange
        MotivationRequestDto request = CreateTestMotivationRequest();
        string aiResponse = @"
Keep up the great work!

Tips:
1. Set small daily goals that are achievable
2. Track your progress to see improvements
3. Reward yourself for consistency
4. This tip should be ignored as we limit to 3
";

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(aiResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

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
        MotivationRequestDto request = CreateTestMotivationRequest();
        string aiResponse = @"
Excellent progress!

Actionable tips:
• Focus on consistency over perfection
- Celebrate small victories daily
* Remember why you started this journey
";

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(aiResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

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
        MotivationRequestDto request = CreateTestMotivationRequest();
        string longMessage = string.Join(". ", Enumerable.Repeat("This is a very long motivational sentence that goes on and on", 10));
        string aiResponse = $"{longMessage}. Quote: \"Test quote\"";

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(aiResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

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
        MotivationRequestDto request = CreateTestMotivationRequest("TestAthlete");
        HashSet<string> messages = new HashSet<string>();

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Service down"));

        // Act - Generate multiple fallback messages
        for (int i = 0; i < 10; i++)
        {
            MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);
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
        MotivationRequestDto request = CreateTestMotivationRequest();

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

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
        MotivationRequestDto request = new MotivationRequestDto
        {
            AthleteProfile = new AthleteProfileDto
            {
                Name = "María José-Smith",
                FitnessLevel = "Intermediate",
                PrimaryGoal = "General Fitness",
            },
        };

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync("¡Excelente trabajo, María!");

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.Contains("María", result.MotivationalMessage);
    }

    [Fact]
    public async Task ExtractTips_WithVeryShortTips_FiltersOut()
    {
        // Arrange
        MotivationRequestDto request = CreateTestMotivationRequest();
        string aiResponse = @"
Tips:
1. Go! 
2. This tip is long enough to be included in the results
3. Yes
4. Another good tip that meets the minimum length requirement
";

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(aiResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result.ActionableTips);
        Assert.Equal(2, result.ActionableTips.Count); // Only the long enough tips
        Assert.All(result.ActionableTips, tip => Assert.True(tip.Length > 15));
    }

    [Fact]
    public async Task ExtractQuote_WithTechnicalContent_FiltersOut()
    {
        // Arrange
        MotivationRequestDto request = CreateTestMotivationRequest();
        string aiResponse = @"
Great job!
""HTTP 404 error occurred while processing the request""
""Believe in yourself and achieve your dreams""
Keep going!";

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(aiResponse);

        // Act
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("Believe in yourself and achieve your dreams", result.Quote);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GenerateMotivationAsync_WithVeryLargeAIResponse_HandlesEfficiently()
    {
        // Arrange
        MotivationRequestDto request = CreateTestMotivationRequest();
        string largeResponse = string.Join("\n", Enumerable.Repeat("This is a line of AI response text.", 1000));

        this.mockAIPromptService
            .Setup(s => s.GetMotivationAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(largeResponse);

        // Act
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        MotivationResponseDto result = await this.service.GenerateMotivationAsync(request, CancellationToken.None);
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
            UpcomingWorkoutType = null,
        };
    }

    private static AthleteProfileDto CreateTestAthleteProfile(string name = "Test User")
    {
        return new AthleteProfileDto
        {
            Id = "123",
            Name = name,
            FitnessLevel = "Intermediate",
            PrimaryGoal = "General Fitness",
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