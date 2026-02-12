# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY VoiceProcessor.sln ./
COPY src/VoiceProcessor.Clients.Api/VoiceProcessor.Clients.Api.csproj src/VoiceProcessor.Clients.Api/
COPY src/VoiceProcessor.Domain/VoiceProcessor.Domain.csproj src/VoiceProcessor.Domain/
COPY src/VoiceProcessor.Managers/VoiceProcessor.Managers.csproj src/VoiceProcessor.Managers/
COPY src/VoiceProcessor.Engines/VoiceProcessor.Engines.csproj src/VoiceProcessor.Engines/
COPY src/VoiceProcessor.Accessors/VoiceProcessor.Accessors.csproj src/VoiceProcessor.Accessors/
COPY src/VoiceProcessor.Utilities/VoiceProcessor.Utilities.csproj src/VoiceProcessor.Utilities/
COPY tests/VoiceProcessor.Managers.Tests/VoiceProcessor.Managers.Tests.csproj tests/VoiceProcessor.Managers.Tests/
COPY tests/VoiceProcessor.Engines.Tests/VoiceProcessor.Engines.Tests.csproj tests/VoiceProcessor.Engines.Tests/
COPY tests/VoiceProcessor.Accessors.Tests/VoiceProcessor.Accessors.Tests.csproj tests/VoiceProcessor.Accessors.Tests/
RUN dotnet restore

# Copy everything and build
COPY . .
RUN dotnet publish src/VoiceProcessor.Clients.Api/VoiceProcessor.Clients.Api.csproj \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Railway injects PORT env var
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "VoiceProcessor.Clients.Api.dll"]
