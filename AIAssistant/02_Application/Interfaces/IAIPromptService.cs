using AIAssistant._03_Infrastructure.OpenAI.Models;

namespace AIAssistant._02_Application.Interfaces;

public interface IAIPromptService
{
    Task<string> GetOpenAICompletionAsync(string prompt);
    Task<string> GetClaudeCompletionAsync(string prompt);
    Task<string> GetOpenAICompletionAsync(List<Message> messages);
    Task<string> GetClaudeCompletionAsync(List<Message> messages);
}
