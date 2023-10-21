using Microsoft.Extensions.DependencyInjection;

using FileShare.Application.Services;

namespace FileShare.Application.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileService, FileService>();

        return services;
    }
}