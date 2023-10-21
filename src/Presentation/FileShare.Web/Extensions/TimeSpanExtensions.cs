static class ElapsedTimeExtensions
{
    public static string GetInStringInterpretation(this TimeSpan elapsedTime) => elapsedTime switch
    {
        { TotalSeconds: < 1 } => "less than a second",
        { TotalSeconds: < 2 } => "about a second",
        { TotalSeconds: < 60 } => $"{elapsedTime.TotalSeconds:0} seconds",
        { TotalMinutes: < 2 } => "about a minute",
        { TotalMinutes: < 60 } => $"{elapsedTime.TotalMinutes:0} minutes",
        { TotalHours: < 2 } => "about an hour",
        { TotalHours: < 24 } => $"{elapsedTime.TotalHours:0} hours",
        { TotalDays: < 2 } => "about a day",
        { TotalDays: < 30 } => $"{elapsedTime.TotalDays:0} days",
        { TotalDays: < 60 } => "about a month",
        { TotalDays: < 365 } => $"{elapsedTime.TotalDays / 30:0} months",
        { TotalDays: < 730 } => "about a year",
        _ => $"{elapsedTime.TotalDays / 365:0} years"
    };
}