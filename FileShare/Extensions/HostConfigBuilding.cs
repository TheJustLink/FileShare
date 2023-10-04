using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

using ConfigurationSubstitution;

namespace FileShare.Extensions;

public static class HostConfigBuilding
{
    public static WebApplicationBuilder SetupConfiguration(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var environmentName = builder.Environment.EnvironmentName;
        var machineName = Environment.MachineName;

        configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configuration.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
        configuration.AddJsonFile($"appsettings.{machineName}.json", optional: true, reloadOnChange: true);

        configuration.AddEnvironmentVariables();

        configuration.EnableSubstitutions("${", "}");

        return builder;
    }
}