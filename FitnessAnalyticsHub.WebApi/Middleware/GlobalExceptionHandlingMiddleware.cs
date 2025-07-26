namespace FitnessAnalyticsHub.WebApi.Middleware
{
    using System.Net;
    using System.Text.Json;
    using FitnessAnalyticsHub.Domain.Exceptions.Activities;
    using FitnessAnalyticsHub.Domain.Exceptions.Athletes;
    using FitnessAnalyticsHub.Infrastructure.Exceptions;
    using FitnessAnalyticsHub.WebApi.Middleware.Models;

    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            HttpResponse response = context.Response;
            response.ContentType = "application/json";

            ErrorResponse errorResponse = exception switch
            {
                ActivityNotFoundException ex => new ErrorResponse
                {
                    Type = "ActivityNotFound",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Details = $"ActivityId: {ex.ActivityId}",
                },

                AthleteNotFoundException ex => new ErrorResponse
                {
                    Type = "AthleteNotFound",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Details = $"AthleteId: {ex.AthleteId}",
                },

                // AI Assistant Exceptions
                AIAssistantApiException ex => new ErrorResponse
                {
                    Type = "AIAssistantApiError",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Details = $"AI Assistant API returned status code: {ex.StatusCode}",
                },

                // Strava Konfigurationsfehler
                StravaConfigurationException ex => new ErrorResponse
                {
                    Type = "StravaConfigurationError",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Details = "Please check your Strava API configuration",
                },

                // Strava Autorisierungsfehler
                StravaAuthorizationException ex => new ErrorResponse
                {
                    Type = "StravaAuthorizationError",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Details = "Authorization process failed",
                },

                // Ungültiger Strava Token
                InvalidStravaTokenException ex => new ErrorResponse
                {
                    Type = "InvalidStravaToken",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Details = "Please check your Strava access token",
                },

                // Strava API Fehler
                StravaApiException ex => new ErrorResponse
                {
                    Type = "StravaApiError",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Details = $"Strava API returned status code: {ex.StatusCode}",
                },

                // Allgemeine Strava Service Fehler
                StravaServiceException ex => new ErrorResponse
                {
                    Type = "StravaServiceError",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Details = ex.InnerException?.Message,
                },

                // Fallback für alle anderen Exceptions
                _ => new ErrorResponse
                {
                    Type = "InternalServerError",
                    Message = "An error occurred while processing your request.",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Details = exception.Message,
                }
            };

            response.StatusCode = errorResponse.StatusCode;

            string jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            return response.WriteAsync(jsonResponse);
        }
    }
}