using AIAssistant.Application.DTOs;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAssistant.Tests.Helpers
{
    public static class GrpcTestScenarios
    {
        /// <summary>
        /// Szenario: Neuer Benutzer ohne Trainingsdaten
        /// </summary>
        public static class NewUser
        {
            public static global::Fitnessanalyticshub.MotivationRequest CreateMotivationRequest()
            {
                return new global::Fitnessanalyticshub.MotivationRequest
                {
                    AthleteProfile = new global::Fitnessanalyticshub.AthleteProfile
                    {
                        Name = "New User",
                        FitnessLevel = "Beginner",
                        PrimaryGoal = "Get Started"
                    }
                };
            }

            public static MotivationResponseDto CreateExpectedResponse()
            {
                return new MotivationResponseDto
                {
                    MotivationalMessage = "Welcome to your fitness journey! Every expert was once a beginner.",
                    Quote = "The journey of a thousand miles begins with one step.",
                    ActionableTips = new List<string>
                {
                    "Start with 15-20 minute walks",
                    "Set small, achievable goals",
                    "Track your progress daily"
                },
                    GeneratedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Szenario: Erfahrener Athlet mit intensivem Training
        /// </summary>
        public static class ExperiencedAthlete
        {
            public static global::Fitnessanalyticshub.WorkoutAnalysisRequest CreateAnalysisRequest()
            {
                var request = new global::Fitnessanalyticshub.WorkoutAnalysisRequest
                {
                    PreferredAiProvider = "googlegemini",
                    AnalysisType = "Performance"
                };

                // Hochintensive Workouts
                request.RecentWorkouts.Add(new global::Fitnessanalyticshub.Workout
                {
                    Date = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"),
                    ActivityType = "Run",
                    Distance = 21100, // Halbmarathon
                    Duration = 5400,  // 1.5 Stunden
                    Calories = 1400
                });

                return request;
            }

            public static WorkoutAnalysisResponseDto CreateExpectedResponse()
            {
                return new WorkoutAnalysisResponseDto
                {
                    Analysis = "Excellent endurance performance! Your half-marathon time shows strong aerobic capacity.",
                    KeyInsights = new List<string>
                {
                    "Consistent pacing throughout the distance",
                    "High caloric expenditure indicates strong effort",
                    "Recovery patterns suggest good fitness base"
                },
                    Recommendations = new List<string>
                {
                    "Consider adding speed work to improve race times",
                    "Incorporate strength training for injury prevention",
                    "Plan recovery weeks to prevent overtraining"
                },
                    Provider = "GoogleGemini",
                    GeneratedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Szenario: Service-Ausfall und Fehlerbehandlung
        /// </summary>
        public static class ServiceFailure
        {
            public static Exception CreateServiceException(string serviceName = "AI Service")
            {
                return new InvalidOperationException($"{serviceName} is temporarily unavailable");
            }

            public static void AssertCorrectErrorHandling(RpcException exception, string expectedService)
            {
                Assert.Equal(StatusCode.Internal, exception.StatusCode);
                Assert.Contains("Failed to generate", exception.Status.Detail);
                Assert.Contains(expectedService, exception.Status.Detail);
            }
        }
    }
}
