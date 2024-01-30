namespace FileShare.Persistence.Minio.Configuration;

public sealed record Settings
{
    public string Uri { get; set; } = null!;
    public string FilesBucket { get; set; } = null!;
}