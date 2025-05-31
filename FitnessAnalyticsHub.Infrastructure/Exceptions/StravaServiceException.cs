using System;

namespace FitnessAnalyticsHub.Infrastructure.Exceptions
{
    public class StravaServiceException : Exception
    {
        public StravaServiceException(string message) : base(message)
        {
        }

        public StravaServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class InvalidStravaTokenException : StravaServiceException
    {
        public InvalidStravaTokenException()
            : base("Invalid or expired Strava access token")
        {
        }

        public InvalidStravaTokenException(string message) : base(message)
        {
        }
    }

    public class StravaApiException : StravaServiceException
    {
        public StravaApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }

    public class StravaConfigurationException : StravaServiceException
    {
        public StravaConfigurationException(string message)
            : base($"Strava configuration error: {message}")
        {
        }
    }

    public class StravaAuthorizationException : StravaServiceException
    {
        public StravaAuthorizationException(string message)
            : base($"Strava authorization error: {message}")
        {
        }
    }
}