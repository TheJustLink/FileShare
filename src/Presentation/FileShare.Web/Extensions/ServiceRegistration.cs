namespace FileShare.Web.Extensions;

static class ServiceRegistration
{
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddControllersWithViews();
        RegisterSwagger(services);

        return services;
    }

    private static void RegisterSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}