using Microsoft.AspNetCore.Builder;
using FileShare.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.SetupConfiguration();
builder.RegisterServices();

var app = builder.Build();
app.Configure();
app.Run();