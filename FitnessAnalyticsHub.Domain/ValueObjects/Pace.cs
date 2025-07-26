namespace FitnessAnalyticsHub.Domain.ValueObjects;

public class Pace : ValueObject
{
    public TimeSpan ValuePerKilometer { get; private set; }

    private Pace()
    {
    } // Für EF Core

    public Pace(TimeSpan valuePerKilometer)
    {
        if (valuePerKilometer <= TimeSpan.Zero)
        {
            throw new ArgumentException("Pace must be positive");
        }

        this.ValuePerKilometer = valuePerKilometer;
    }

    public static Pace FromDistanceAndDuration(double distance, TimeSpan duration)
    {
        double distanceInKm = distance;
        double timePerKm = duration.TotalSeconds / (double)distanceInKm;
        return new Pace(TimeSpan.FromSeconds(timePerKm));
    }

    // Helper für Anzeige
    public string ToDisplayString()
    {
        return $"{this.ValuePerKilometer:mm\\:ss} min/km";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return this.ValuePerKilometer;
    }
}
