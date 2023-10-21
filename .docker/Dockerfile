# Local working directory should be the root of src

ARG ProjectName="FileShare.Web"
ARG ProjectDirectory="Presentation/$ProjectName"
ARG ProjectPath="$ProjectDirectory/$ProjectName.csproj"
ARG ProjectImage="$ProjectName.dll"

# Prepare projects restore

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS prepare-restore
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global --no-cache dotnet-subset

WORKDIR /app/src
COPY . .
ARG ProjectPath
RUN dotnet subset restore $ProjectPath --root-directory . --output restore_subset/

# SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS publish
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8
WORKDIR /app/src

# Restore deps for caching
COPY --from=prepare-restore /app/src/restore_subset .
ARG ProjectPath
RUN dotnet restore $ProjectPath

# Copy and build app and libraries
COPY . .
ARG ProjectDirectory
WORKDIR $ProjectDirectory
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 as final

# Move compiled app to base runtime image
WORKDIR /app
COPY --from=publish /app/publish .

# Entrypoint
EXPOSE 80
ARG ProjectImage
ENV ProjectImage=$ProjectImage
ENTRYPOINT dotnet ${ProjectImage}