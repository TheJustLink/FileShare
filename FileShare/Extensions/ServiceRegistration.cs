using System;
using FileShare.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Minio.AspNetCore;

namespace FileShare.Extensions;

public static class ServiceRegistration
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var settings = builder.Configuration
            .GetRequiredSection(nameof(Settings))
            .Get<Settings>()!;

        services.AddControllersWithViews();

        RegisterSwagger(services);
        RegisterMinio(services, settings.MinioConnectionString);

        return builder;
    }

    private static void RegisterSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
    private static void RegisterMinio(IServiceCollection services, string connectionString)
    {
        services.AddMinio(new Uri(connectionString));
    }
}