namespace FitnessAnalyticsHub.Domain.Exceptions.Activities
{
    public class ActivityNotFoundException : Exception
    {
        public ActivityNotFoundException(int activityId)
            : base($"Activity with ID {activityId} not found")
        {
            this.ActivityId = activityId;
        }

        public int ActivityId { get; }
    }
}
