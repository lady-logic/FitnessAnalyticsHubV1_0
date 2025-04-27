namespace AIAssistant._03_Infrastructure.OpenAI.Models
{
    public class OpenAIRequest
    {
        public string Model { get; set; }
        public List<Message> Messages { get; set; }
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
    }
}
