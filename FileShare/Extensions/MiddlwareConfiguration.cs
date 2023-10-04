using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace FileShare.Extensions;

public static class MiddlwareConfiguration
{
    public static WebApplication Configure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            ConfigureSwagger(app);
        }
        else
        {
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}"
        );

        return app;
    }

    private static void ConfigureSwagger(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            var url = "/swagger/v1/swagger.json";
            var appName = $"{app.Environment.ApplicationName} v1";

            c.SwaggerEndpoint(url, appName);
        });
    }
}