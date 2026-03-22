# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy project files first for layer caching
COPY src/backend/Directory.Build.props ./src/backend/
COPY src/backend/Blend.Api/Blend.Api.csproj ./src/backend/Blend.Api/
COPY src/backend/Blend.ServiceDefaults/Blend.ServiceDefaults.csproj ./src/backend/Blend.ServiceDefaults/

# Restore only the API and its dependencies (not AppHost/tests)
RUN dotnet restore src/backend/Blend.Api/Blend.Api.csproj

# Copy remaining source files
COPY src/backend/Blend.Api/ ./src/backend/Blend.Api/
COPY src/backend/Blend.ServiceDefaults/ ./src/backend/Blend.ServiceDefaults/

# Build and publish in Release configuration
RUN dotnet publish src/backend/Blend.Api/Blend.Api.csproj \
    --no-restore \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS runtime
WORKDIR /app

# Create a non-root user for security
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup --no-create-home appuser

# Copy published application
COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

# Health check using the /healthz liveness endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
    CMD wget -qO- http://localhost:8080/healthz || exit 1

# Expose HTTP port (Azure Container Apps default is 8080)
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

ENTRYPOINT ["dotnet", "Blend.Api.dll"]
