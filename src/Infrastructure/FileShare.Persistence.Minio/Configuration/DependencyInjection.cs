using FileShare.Application.Repositories;
using FileShare.Persistence.Minio.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Minio;
using Minio.AspNetCore;

namespace FileShare.Persistence.Minio.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddMinioServices(this IServiceCollection services, IConfiguration configuration)
    {
        var settingsConfig = configuration.GetRequiredSection(nameof(Minio));
        services.Configure<Settings>(settingsConfig);

        var settings = settingsConfig.Get<Settings>()!;
        services.AddMinio(new Uri(settings.Uri));

        services.AddSingleton<BucketContext>();
        services.AddSingleton<IFileMetadataRepository, FileMetadataRepository>();
        services.AddSingleton<IFileContentRepository, FileContentRepository>();

        return services;
    }
}