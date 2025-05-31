using FitnessAnalyticsHub.Domain.Exceptions.Activities;
using FitnessAnalyticsHub.Domain.Exceptions.Athletes;
using FitnessAnalyticsHub.Infrastructure.Exceptions;
using FitnessAnalyticsHub.WebApi.Middleware.Models;
using System.Net;
using System.Text.Json;

namespace FitnessAnalyticsHub.WebApi.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = exception switch
            {
                ActivityNotFoundException ex => new ErrorResponse
                {
                    Type = "ActivityNotFound",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Details = $"ActivityId: {ex.ActivityId}"
                },

                AthleteNotFoundException ex => new ErrorResponse
                {
                    Type = "AthleteNotFound",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Details = $"AthleteId: {ex.AthleteId}"
                },

                // Strava Konfigurationsfehler
                StravaConfigurationException ex => new ErrorResponse
                {
                    Type = "StravaConfigurationError",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Details = "Please check your Strava API configuration"
                },

                // Strava Autorisierungsfehler
                StravaAuthorizationException ex => new ErrorResponse
                {
                    Type = "StravaAuthorizationError",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Details = "Authorization process failed"
                },

                // Ungültiger Strava Token
                InvalidStravaTokenException ex => new ErrorResponse
                {
                    Type = "InvalidStravaToken",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Details = "Please check your Strava access token"
                },

                // Strava API Fehler
                StravaApiException ex => new ErrorResponse
                {
                    Type = "StravaApiError",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Details = $"Strava API returned status code: {ex.StatusCode}"
                },

                // Allgemeine Strava Service Fehler
                StravaServiceException ex => new ErrorResponse
                {
                    Type = "StravaServiceError",
                    Message = ex.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Details = ex.InnerException?.Message
                },

                // Fallback für alle anderen Exceptions
                _ => new ErrorResponse
                {
                    Type = "InternalServerError",
                    Message = "An error occurred while processing your request.",
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Details = exception.Message
                }
            };

            response.StatusCode = errorResponse.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteAsync(jsonResponse);
        }
    }
}