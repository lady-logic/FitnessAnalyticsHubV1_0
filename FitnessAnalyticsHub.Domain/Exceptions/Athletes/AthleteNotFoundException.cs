namespace FitnessAnalyticsHub.Domain.Exceptions.Athletes
{
    public class AthleteNotFoundException : Exception
    {
        public AthleteNotFoundException(int athleteId)
            : base($"Athlete with ID {athleteId} not found")
        {
            AthleteId = athleteId;
        }

        public int AthleteId { get; }
    }
}
