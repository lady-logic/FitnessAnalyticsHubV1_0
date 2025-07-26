namespace AIAssistant.Tests.UI.API.Services
{
    using AIAssistant.Application.DTOs;
    using AIAssistant.Application.Interfaces;
    using AIAssistant.Applications.DTOs;
    using FitnessAnalyticsHub.AIAssistant.UI.API.Services;
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using Moq;

    public class MotivationGrpcServiceTests
    {
        private readonly Mock<IMotivationCoachService> mockMotivationService;
        private readonly Mock<ILogger<MotivationGrpcService>> mockLogger;
        private readonly MotivationGrpcService service;

        public MotivationGrpcServiceTests()
        {
            this.mockMotivationService = new Mock<IMotivationCoachService>();
            this.mockLogger = new Mock<ILogger<MotivationGrpcService>>();
            this.service = new MotivationGrpcService(this.mockMotivationService.Object, this.mockLogger.Object);
        }

        #region GetMotivation Tests

        [Fact]
        public async Task GetMotivation_WithValidRequest_ReturnsCorrectResponse()
        {
            // Arrange
            Fitnessanalyticshub.MotivationRequest grpcRequest = new global::Fitnessanalyticshub.MotivationRequest
            {
                AthleteProfile = new global::Fitnessanalyticshub.AthleteProfile
                {
                    Name = "Test Athlete",
                    FitnessLevel = "Intermediate",
                    PrimaryGoal = "Weight Loss",
                },
            };

            MotivationResponseDto serviceResponse = new MotivationResponseDto
            {
                MotivationalMessage = "You're doing amazing! Keep pushing forward!",
                Quote = "Success is the sum of small efforts repeated day in and day out.",
                ActionableTips = new List<string>
            {
                "Set realistic daily goals",
                "Track your progress weekly",
                "Celebrate small victories",
            },
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockMotivationService
                .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.MotivationResponse result = await this.service.GetMotivation(grpcRequest, context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("You're doing amazing! Keep pushing forward!", result.MotivationalMessage);
            Assert.Equal("Success is the sum of small efforts repeated day in and day out.", result.Quote);
            Assert.Equal(3, result.ActionableTips.Count);
            Assert.Contains("Set realistic daily goals", result.ActionableTips);
            Assert.Contains("Track your progress weekly", result.ActionableTips);
            Assert.Contains("Celebrate small victories", result.ActionableTips);
            Assert.False(string.IsNullOrEmpty(result.GeneratedAt));
        }

        [Fact]
        public async Task GetMotivation_WithNullAthleteProfile_ThrowsRpcException()
        {
            // Arrange
            Fitnessanalyticshub.MotivationRequest grpcRequest = new global::Fitnessanalyticshub.MotivationRequest
            {
                AthleteProfile = null, // Dies wird eine NullReferenceException in der Extension verursachen
            };

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act & Assert
            RpcException exception = await Assert.ThrowsAsync<RpcException>(() =>
                this.service.GetMotivation(grpcRequest, context));

            // Das erwartete Verhalten: RpcException mit Internal Status
            Assert.Equal(StatusCode.Internal, exception.StatusCode);
            Assert.Contains("Failed to generate motivation", exception.Status.Detail);
            Assert.Contains("Object reference not set to an instance of an object", exception.Status.Detail);

            // Verify error was logged
            this.mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error generating motivation")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetMotivation_WithEmptyResponseFields_HandlesNullValues()
        {
            // Arrange
            Fitnessanalyticshub.MotivationRequest grpcRequest = new global::Fitnessanalyticshub.MotivationRequest
            {
                AthleteProfile = new global::Fitnessanalyticshub.AthleteProfile
                {
                    Name = "Test User",
                },
            };

            MotivationResponseDto serviceResponse = new MotivationResponseDto
            {
                MotivationalMessage = null, // Null value test
                Quote = null,
                ActionableTips = null,
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockMotivationService
                .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.MotivationResponse result = await this.service.GetMotivation(grpcRequest, context);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.MotivationalMessage); // Should be empty string, not null
            Assert.Equal(string.Empty, result.Quote);
            Assert.Empty(result.ActionableTips); // Should be empty collection
        }

        [Fact]
        public async Task GetMotivation_CallsServiceWithCorrectMapping()
        {
            // Arrange
            Fitnessanalyticshub.MotivationRequest grpcRequest = new global::Fitnessanalyticshub.MotivationRequest
            {
                AthleteProfile = new global::Fitnessanalyticshub.AthleteProfile
                {
                    Name = "Mapping Test User",
                    FitnessLevel = "Advanced",
                    PrimaryGoal = "Marathon Training",
                },
            };

            MotivationResponseDto serviceResponse = new MotivationResponseDto
            {
                MotivationalMessage = "Test message",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockMotivationService
                .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            await this.service.GetMotivation(grpcRequest, context);

            // Assert - Verify the service was called with correct mapped data
            this.mockMotivationService.Verify(
                s => s.GetHuggingFaceMotivationalMessageAsync(
                    It.Is<MotivationRequestDto>(req =>
                        req.AthleteProfile.Name == "Mapping Test User" &&
                        req.AthleteProfile.FitnessLevel == "Advanced" &&
                        req.AthleteProfile.PrimaryGoal == "Marathon Training"), CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task GetMotivation_WhenServiceThrowsException_ThrowsRpcException()
        {
            // Arrange
            Fitnessanalyticshub.MotivationRequest grpcRequest = new global::Fitnessanalyticshub.MotivationRequest
            {
                AthleteProfile = new global::Fitnessanalyticshub.AthleteProfile
                {
                    Name = "Error Test User",
                },
            };

            this.mockMotivationService
                .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("Service is down"));

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act & Assert
            RpcException exception = await Assert.ThrowsAsync<RpcException>(() =>
                this.service.GetMotivation(grpcRequest, context));

            Assert.Equal(StatusCode.Internal, exception.StatusCode);
            Assert.Contains("Failed to generate motivation", exception.Status.Detail);
            Assert.Contains("Service is down", exception.Status.Detail);

            // Verify error was logged
            this.mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error generating motivation")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetMotivation_LogsRequestAndResponse()
        {
            // Arrange
            Fitnessanalyticshub.MotivationRequest grpcRequest = new global::Fitnessanalyticshub.MotivationRequest
            {
                AthleteProfile = new global::Fitnessanalyticshub.AthleteProfile
                {
                    Name = "Logging Test User",
                },
            };

            MotivationResponseDto serviceResponse = new MotivationResponseDto
            {
                MotivationalMessage = "Test message",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockMotivationService
                .Setup(s => s.GetHuggingFaceMotivationalMessageAsync(It.IsAny<MotivationRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            await this.service.GetMotivation(grpcRequest, context);

            // Assert - Verify request logging
            this.mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Received motivation request for athlete: Logging Test User")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify success logging
            this.mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully generated motivation response")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        #endregion
    }
}
