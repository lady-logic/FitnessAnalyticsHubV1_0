namespace FitnessAnalyticsHub.Infrastructure.Exceptions
{
    public class AIAssistantApiException : Exception
    {
        public AIAssistantApiException(string message, int statusCode)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        public AIAssistantApiException(string message, int statusCode, Exception innerException)
            : base(message, innerException)
        {
            this.StatusCode = statusCode;
        }

        public int StatusCode { get; }
    }
}