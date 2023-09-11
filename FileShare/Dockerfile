# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

# SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8
WORKDIR /app/src

# Restore deps for caching
COPY *.csproj .
RUN dotnet restore
COPY . .

# Build
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Move compiled app to base runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Entrypoint
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "FileShare.dll"]