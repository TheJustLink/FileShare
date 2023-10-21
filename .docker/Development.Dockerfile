# SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS dev-env
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8

# Entrypoint
EXPOSE 80
WORKDIR /app/Presentation/FileShare.Web
ENTRYPOINT dotnet watch -v run --urls=http://+:80 --non-interactive