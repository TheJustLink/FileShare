using FileShare.Persistence.Minio.Configuration;
using FileShare.Application.Configuration;
using FileShare.Web.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.SetupConfiguration();

builder.Services.RegisterServices();
builder.Services.AddMinioServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();
app.Configure();

await app.Services.InitializeMinioContextAsync();

app.Run();