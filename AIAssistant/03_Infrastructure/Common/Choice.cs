namespace AIAssistant._03_Infrastructure.OpenAI.Models
{
    public class Choice
    {
        public Message Message { get; set; } = new Message();
        public int Index { get; set; }
        public string? FinishReason { get; set; }
    }
}
