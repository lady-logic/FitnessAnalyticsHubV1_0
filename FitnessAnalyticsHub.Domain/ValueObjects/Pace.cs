namespace FitnessAnalyticsHub.Domain.ValueObjects;

public class Pace : ValueObject
{
    public TimeSpan ValuePerKilometer { get; private set; }

    private Pace() { } // Für EF Core

    public Pace(TimeSpan valuePerKilometer)
    {
        if (valuePerKilometer <= TimeSpan.Zero)
            throw new ArgumentException("Pace must be positive");
        ValuePerKilometer = valuePerKilometer;
    }

    public static Pace FromDistanceAndDuration(double distance, TimeSpan duration)
    {
        var distanceInKm = distance;
        var timePerKm = duration.TotalSeconds / (double)distanceInKm;
        return new Pace(TimeSpan.FromSeconds(timePerKm));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ValuePerKilometer;
    }
}
