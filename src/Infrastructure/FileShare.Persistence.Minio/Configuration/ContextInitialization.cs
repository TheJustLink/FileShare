using Microsoft.Extensions.DependencyInjection;

namespace FileShare.Persistence.Minio.Configuration;

public static class ContextInitialization
{
    public static Task InitializeMinioContextAsync(this IServiceProvider services)
    {
        var context = services.GetRequiredService<BucketContext>();

        return context.EnsureBucketExistsAsync();
    }
}