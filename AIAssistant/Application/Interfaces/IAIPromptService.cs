namespace AIAssistant.Application.Interfaces;

public interface IAIPromptService
{
    Task<string> GetFitnessAnalysisAsync(string prompt, CancellationToken cancellationToken);

    Task<string> GetHealthAnalysisAsync(string prompt, CancellationToken cancellationToken);

    Task<string> GetMotivationAsync(string prompt, CancellationToken cancellationToken);
}
