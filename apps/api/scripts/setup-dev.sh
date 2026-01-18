#!/bin/bash

# Development environment setup script

set -e

echo "=== VoiceProcessor API - Development Setup ==="

# Check prerequisites
command -v dotnet >/dev/null 2>&1 || { echo "Error: .NET SDK is required but not installed."; exit 1; }
command -v docker >/dev/null 2>&1 || { echo "Error: Docker is required but not installed."; exit 1; }

echo "✓ Prerequisites checked"

# Start database and redis
echo "Starting PostgreSQL and Redis..."
docker-compose up -d db redis

# Wait for database to be ready
echo "Waiting for database to be ready..."
until docker-compose exec -T db pg_isready -U postgres > /dev/null 2>&1; do
    sleep 1
done

echo "✓ Database is ready"

# Restore packages
echo "Restoring NuGet packages..."
dotnet restore

# Apply migrations (when available)
# echo "Applying database migrations..."
# dotnet ef database update --project src/VoiceProcessor.Infrastructure --startup-project src/VoiceProcessor.Api

echo ""
echo "=== Setup complete ==="
echo "Run 'dotnet run --project src/VoiceProcessor.Api' to start the API"
