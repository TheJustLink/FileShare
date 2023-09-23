# SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS dev-env

ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8
WORKDIR /app

# Create anonymous volumes to avoid conflicts
VOLUME /app/bin
VOLUME /app/obj

# Entrypoint
EXPOSE 80
ENTRYPOINT dotnet watch -v run --urls=http://+:80