# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY apps/api/VoiceProcessor.sln ./
COPY apps/api/src/VoiceProcessor.Clients.Api/VoiceProcessor.Clients.Api.csproj src/VoiceProcessor.Clients.Api/
COPY apps/api/src/VoiceProcessor.Domain/VoiceProcessor.Domain.csproj src/VoiceProcessor.Domain/
COPY apps/api/src/VoiceProcessor.Managers/VoiceProcessor.Managers.csproj src/VoiceProcessor.Managers/
COPY apps/api/src/VoiceProcessor.Engines/VoiceProcessor.Engines.csproj src/VoiceProcessor.Engines/
COPY apps/api/src/VoiceProcessor.Accessors/VoiceProcessor.Accessors.csproj src/VoiceProcessor.Accessors/
COPY apps/api/src/VoiceProcessor.Utilities/VoiceProcessor.Utilities.csproj src/VoiceProcessor.Utilities/
COPY apps/api/tests/VoiceProcessor.Managers.Tests/VoiceProcessor.Managers.Tests.csproj tests/VoiceProcessor.Managers.Tests/
COPY apps/api/tests/VoiceProcessor.Engines.Tests/VoiceProcessor.Engines.Tests.csproj tests/VoiceProcessor.Engines.Tests/
COPY apps/api/tests/VoiceProcessor.Accessors.Tests/VoiceProcessor.Accessors.Tests.csproj tests/VoiceProcessor.Accessors.Tests/
RUN dotnet restore

# Copy everything and build
COPY apps/api/ .
RUN dotnet publish src/VoiceProcessor.Clients.Api/VoiceProcessor.Clients.Api.csproj \
    -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .

# Railway injects PORT env var at runtime
# ASP.NET Core will use PORT if set, otherwise defaults to 8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "VoiceProcessor.Clients.Api.dll"]
