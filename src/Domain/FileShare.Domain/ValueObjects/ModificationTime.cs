namespace FileShare.Domain.ValueObjects;

public readonly record struct ModificationTime(DateTimeOffset Date)
{
    public static ModificationTime Now => new(DateTimeOffset.Now);

    public TimeSpan ElapsedTime => DateTimeOffset.Now - Date;

    public string GetElapsedTimeString()
    {
        var elapsed = ElapsedTime;

        return elapsed switch
        {
            { TotalDays: > 1 } => $"{elapsed.TotalDays:0} days ago",
            { TotalHours: > 1 } => $"{elapsed.TotalHours:0} hours ago",
            { TotalMinutes: > 1 } => $"{elapsed.TotalMinutes:0} minutes ago",
            { TotalSeconds: > 1 } => $"{elapsed.TotalSeconds:0} seconds ago",

            _ => "just now"
        };
    }
}