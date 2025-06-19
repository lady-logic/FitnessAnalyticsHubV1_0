namespace FitnessAnalyticsHub.Infrastructure.Exceptions
{
    public class AIAssistantApiException : Exception
    {
        public AIAssistantApiException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public AIAssistantApiException(string message, int statusCode, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }
}