namespace FileShare.Application.Configuration;

public sealed record Settings
{
    public double MaxFileSizeInMB { get; set; }
}