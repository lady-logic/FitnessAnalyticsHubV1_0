﻿namespace AIAssistant._02_Application.Interfaces;

public interface IAIPromptService
{
    Task<string> GetFitnessAnalysisAsync(string prompt);
    Task<string> GetHealthAnalysisAsync(string prompt);
    Task<string> GetMotivationAsync(string prompt);
}
