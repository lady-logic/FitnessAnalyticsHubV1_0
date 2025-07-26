namespace AIAssistant.Tests.UI.API.Services
{
    using AIAssistant.Application.DTOs;
    using AIAssistant.Application.Interfaces;
    using AIAssistant.UI.API.Services;
    using FitnessAnalyticsHub.AIAssistant.Application.DTOs;
    using Grpc.Core;
    using Microsoft.Extensions.Logging;
    using Moq;

    public class WorkoutAnalysisGrpcServiceTests
    {
        private readonly Mock<IWorkoutAnalysisService> mockWorkoutAnalysisService;
        private readonly Mock<ILogger<WorkoutAnalysisGrpcService>> mockLogger;
        private readonly WorkoutAnalysisGrpcService service;

        public WorkoutAnalysisGrpcServiceTests()
        {
            this.mockWorkoutAnalysisService = new Mock<IWorkoutAnalysisService>();
            this.mockLogger = new Mock<ILogger<WorkoutAnalysisGrpcService>>();
            this.service = new WorkoutAnalysisGrpcService(this.mockWorkoutAnalysisService.Object, this.mockLogger.Object);
        }

        #region GetWorkoutAnalysis Tests

        [Fact]
        public async Task GetWorkoutAnalysis_WithHuggingFaceProvider_CallsCorrectService()
        {
            // Arrange
            Fitnessanalyticshub.WorkoutAnalysisRequest grpcRequest = new global::Fitnessanalyticshub.WorkoutAnalysisRequest
            {
                PreferredAiProvider = "huggingface",
                AnalysisType = "Performance",
            };
            grpcRequest.RecentWorkouts.Add(new global::Fitnessanalyticshub.Workout
            {
                Date = "2025-01-15",
                ActivityType = "Run",
                Distance = 5000,
                Duration = 1800,
                Calories = 350,
            });

            WorkoutAnalysisResponseDto serviceResponse = new WorkoutAnalysisResponseDto
            {
                Analysis = "Great running performance!",
                KeyInsights = new List<string> { "Consistent pace", "Good endurance" },
                Recommendations = new List<string> { "Increase distance gradually" },
                Provider = "HuggingFace",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.WorkoutAnalysisResponse result = await this.service.GetWorkoutAnalysis(grpcRequest, context);

            // Assert
            Assert.Equal("Great running performance!", result.Analysis);
            Assert.Equal(2, result.KeyInsights.Count);
            Assert.Single(result.Recommendations);
            Assert.Equal("HuggingFace", result.Source);

            this.mockWorkoutAnalysisService.Verify(
                s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
                Times.Once);
            this.mockWorkoutAnalysisService.Verify(
                s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task GetWorkoutAnalysis_WithGoogleGeminiProvider_CallsCorrectService()
        {
            // Arrange
            Fitnessanalyticshub.WorkoutAnalysisRequest grpcRequest = new global::Fitnessanalyticshub.WorkoutAnalysisRequest
            {
                PreferredAiProvider = "googlegemini",
                AnalysisType = "Health",
            };

            WorkoutAnalysisResponseDto serviceResponse = new WorkoutAnalysisResponseDto
            {
                Analysis = "Health analysis from GoogleGemini",
                KeyInsights = new List<string> { "Good recovery pattern" },
                Recommendations = new List<string> { "Focus on hydration" },
                Provider = "GoogleGemini",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.WorkoutAnalysisResponse result = await this.service.GetWorkoutAnalysis(grpcRequest, context);

            // Assert
            Assert.Equal("Health analysis from GoogleGemini", result.Analysis);
            Assert.Equal("GoogleGemini", result.Source);

            this.mockWorkoutAnalysisService.Verify(
                s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
                Times.Once);
            this.mockWorkoutAnalysisService.Verify(
                s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task GetWorkoutAnalysis_WithNoProvider_DefaultsToHuggingFace()
        {
            // Arrange
            Fitnessanalyticshub.WorkoutAnalysisRequest grpcRequest = new global::Fitnessanalyticshub.WorkoutAnalysisRequest
            {
                // PreferredAiProvider wird nicht gesetzt (bleibt empty string, nicht null)
            };

            WorkoutAnalysisResponseDto serviceResponse = new WorkoutAnalysisResponseDto
            {
                Analysis = "Default analysis",
                Provider = "HuggingFace",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.WorkoutAnalysisResponse result = await this.service.GetWorkoutAnalysis(grpcRequest, context);

            // Assert
            this.mockWorkoutAnalysisService.Verify(
                s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
                Times.Once);
        }

        [Fact]
        public async Task GetWorkoutAnalysis_WhenServiceThrowsException_ThrowsRpcException()
        {
            // Arrange
            Fitnessanalyticshub.WorkoutAnalysisRequest grpcRequest = new global::Fitnessanalyticshub.WorkoutAnalysisRequest();

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ThrowsAsync(new InvalidOperationException("Analysis service failed"));

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act & Assert
            RpcException exception = await Assert.ThrowsAsync<RpcException>(() =>
                this.service.GetWorkoutAnalysis(grpcRequest, context));

            Assert.Equal(StatusCode.Internal, exception.StatusCode);
            Assert.Contains("Failed to generate workout analysis", exception.Status.Detail);
            Assert.Contains("Analysis service failed", exception.Status.Detail);
        }

        #endregion

        #region GetPerformanceTrends Tests

        [Fact]
        public async Task GetPerformanceTrends_WithValidRequest_ReturnsAnalysis()
        {
            // Arrange
            Fitnessanalyticshub.PerformanceTrendsRequest grpcRequest = new global::Fitnessanalyticshub.PerformanceTrendsRequest
            {
                AthleteId = 123,
                TimeFrame = "month",
            };

            WorkoutAnalysisResponseDto serviceResponse = new WorkoutAnalysisResponseDto
            {
                Analysis = "Performance trends show improvement",
                KeyInsights = new List<string> { "Consistent training", "Progressive overload" },
                Recommendations = new List<string> { "Maintain current routine" },
                Provider = "HuggingFace",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.WorkoutAnalysisResponse result = await this.service.GetPerformanceTrends(grpcRequest, context);

            // Assert
            Assert.Equal("Performance trends show improvement", result.Analysis);
            Assert.Equal("PerformanceTrends", result.AnalysisType);
            Assert.Equal(2, result.KeyInsights.Count);

            // Verify service was called with correct analysis type
            this.mockWorkoutAnalysisService.Verify(
                s => s.AnalyzeHuggingFaceWorkoutsAsync(
                    It.Is<WorkoutAnalysisRequestDto>(req =>
                        req.AnalysisType == "Trends" &&
                        req.AdditionalContext!.ContainsKey("athleteId") &&
                        req.AdditionalContext["athleteId"].Equals(123) &&
                        req.AdditionalContext.ContainsKey("timeFrame") &&
                        req.AdditionalContext["timeFrame"].Equals("month")), CancellationToken.None),
                Times.Once);
        }

        #endregion

        #region GetTrainingRecommendations Tests

        [Fact]
        public async Task GetTrainingRecommendations_WithValidRequest_ReturnsRecommendations()
        {
            // Arrange
            Fitnessanalyticshub.TrainingRecommendationsRequest grpcRequest = new global::Fitnessanalyticshub.TrainingRecommendationsRequest
            {
                AthleteId = 456,
            };

            WorkoutAnalysisResponseDto serviceResponse = new WorkoutAnalysisResponseDto
            {
                Analysis = "Training recommendations analysis",
                Recommendations = new List<string> { "Add strength training", "Improve flexibility" },
                Provider = "HuggingFace",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.WorkoutAnalysisResponse result = await this.service.GetTrainingRecommendations(grpcRequest, context);

            // Assert
            Assert.Equal("TrainingRecommendations", result.AnalysisType);
            Assert.Equal(2, result.Recommendations.Count);

            this.mockWorkoutAnalysisService.Verify(
                s => s.AnalyzeHuggingFaceWorkoutsAsync(
                    It.Is<WorkoutAnalysisRequestDto>(req =>
                        req.AnalysisType == "Recommendations" &&
                        req.AdditionalContext!.ContainsKey("focus") &&
                        req.AdditionalContext["focus"].Equals("training_optimization")), CancellationToken.None),
                Times.Once);
        }

        #endregion

        #region AnalyzeHealthMetrics Tests

        [Fact]
        public async Task AnalyzeHealthMetrics_WithValidRequest_ReturnsHealthAnalysis()
        {
            // Arrange
            Fitnessanalyticshub.HealthAnalysisRequest grpcRequest = new global::Fitnessanalyticshub.HealthAnalysisRequest
            {
                AthleteId = 789,
            };
            grpcRequest.RecentWorkouts.Add(new global::Fitnessanalyticshub.Workout
            {
                Date = "2025-01-15",
                ActivityType = "Run",
                Distance = 5000,
                Duration = 1800,
                Calories = 350,
            });

            WorkoutAnalysisResponseDto serviceResponse = new WorkoutAnalysisResponseDto
            {
                Analysis = "Health metrics are within normal range",
                KeyInsights = new List<string> { "Good cardiovascular health" },
                Provider = "HuggingFace",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.WorkoutAnalysisResponse result = await this.service.AnalyzeHealthMetrics(grpcRequest, context);

            // Assert
            Assert.Equal("HealthMetrics", result.AnalysisType);
            Assert.Equal("Health metrics are within normal range", result.Analysis);

            this.mockWorkoutAnalysisService.Verify(
                s => s.AnalyzeHuggingFaceWorkoutsAsync(
                    It.Is<WorkoutAnalysisRequestDto>(req =>
                        req.AnalysisType == "Health" &&
                        req.AdditionalContext!.ContainsKey("focus") &&
                        req.AdditionalContext["focus"].Equals("injury_prevention")), CancellationToken.None),
                Times.Once);
        }

        #endregion

        #region AnalyzeGoogleGeminiWorkouts Tests

        [Fact]
        public async Task AnalyzeGoogleGeminiWorkouts_WithValidRequest_CallsGoogleGeminiService()
        {
            // Arrange
            Fitnessanalyticshub.WorkoutAnalysisRequest grpcRequest = new global::Fitnessanalyticshub.WorkoutAnalysisRequest
            {
                AnalysisType = "Performance",
            };

            WorkoutAnalysisResponseDto serviceResponse = new WorkoutAnalysisResponseDto
            {
                Analysis = "GoogleGemini analysis result",
                Provider = "GoogleGemini",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.WorkoutAnalysisResponse result = await this.service.AnalyzeGoogleGeminiWorkouts(grpcRequest, context);

            // Assert
            Assert.Equal("GoogleGemini", result.Source);

            this.mockWorkoutAnalysisService.Verify(
                s => s.AnalyzeGoogleGeminiWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None),
                Times.Once);
        }

        #endregion

        #region CheckHealth Tests

        [Fact]
        public async Task CheckHealth_WhenServiceIsHealthy_ReturnsHealthyResponse()
        {
            // Arrange
            Fitnessanalyticshub.HealthCheckRequest grpcRequest = new global::Fitnessanalyticshub.HealthCheckRequest();

            WorkoutAnalysisResponseDto serviceResponse = new WorkoutAnalysisResponseDto
            {
                Analysis = "Health check successful",
                GeneratedAt = DateTime.UtcNow,
            };

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ReturnsAsync(serviceResponse);

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.HealthCheckResponse result = await this.service.CheckHealth(grpcRequest, context);

            // Assert
            Assert.True(result.IsHealthy);
            Assert.Equal("Workout analysis service is responding", result.Message);
            Assert.False(string.IsNullOrEmpty(result.Timestamp));
        }

        [Fact]
        public async Task CheckHealth_WhenServiceFails_ReturnsUnhealthyResponse()
        {
            // Arrange
            Fitnessanalyticshub.HealthCheckRequest grpcRequest = new global::Fitnessanalyticshub.HealthCheckRequest();

            this.mockWorkoutAnalysisService
                .Setup(s => s.AnalyzeHuggingFaceWorkoutsAsync(It.IsAny<WorkoutAnalysisRequestDto>(), CancellationToken.None))
                .ThrowsAsync(new Exception("Service is down"));

            ServerCallContext context = new Mock<ServerCallContext>().Object;

            // Act
            Fitnessanalyticshub.HealthCheckResponse result = await this.service.CheckHealth(grpcRequest, context);

            // Assert
            Assert.False(result.IsHealthy);
            Assert.Contains("Health check failed", result.Message);
            Assert.Contains("Service is down", result.Message);
        }

        #endregion

        #region Helper Method Tests

        [Fact]
        public void GetDemoWorkouts_ReturnsCorrectNumberOfWorkouts()
        {
            // Act - Using reflection to test private method
            System.Reflection.MethodInfo? method = typeof(WorkoutAnalysisGrpcService).GetMethod(
                "GetDemoWorkouts",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            List<WorkoutDataDto>? result = method?.Invoke(this.service, new object[] { 123, "week" }) as List<WorkoutDataDto>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, workout => Assert.NotNull(workout.ActivityType));
        }

        [Fact]
        public void GetDemoAthleteProfile_ReturnsValidProfile()
        {
            // Act - Using reflection to test private method
            System.Reflection.MethodInfo? method = typeof(WorkoutAnalysisGrpcService).GetMethod(
                "GetDemoAthleteProfile",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            AthleteProfileDto? result = method?.Invoke(this.service, new object[] { 123 }) as AthleteProfileDto;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123", result.Id);
            Assert.Equal("Demo User", result.Name);
            Assert.Equal("Intermediate", result.FitnessLevel);
            Assert.NotNull(result.Preferences);
        }

        #endregion
    }
}
