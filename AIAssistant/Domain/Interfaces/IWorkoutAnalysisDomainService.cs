using AIAssistant.Domain.Models;

namespace AIAssistant.Domain.Interfaces;

public interface IWorkoutAnalysisDomainService
{
    string FormatWorkoutDataForAnalysis(IEnumerable<WorkoutData> workouts);

    string FormatAthleteProfileForAnalysis(AthleteProfile profile);
}
