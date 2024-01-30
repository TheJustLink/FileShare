using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using FileShare.Application.Services;

namespace FileShare.Application.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<Settings>(configuration.GetRequiredSection(nameof(Application)));

        services.AddSingleton<IFileService, FileService>();

        return services;
    }
}