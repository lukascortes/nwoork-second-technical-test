# syntax=docker/dockerfile:1
# ── Build stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first to maximise layer caching on `restore`.
COPY ["TimeOffManager.sln", "./"]
COPY ["src/TimeOffManager.Domain/TimeOffManager.Domain.csproj", "src/TimeOffManager.Domain/"]
COPY ["src/TimeOffManager.Application/TimeOffManager.Application.csproj", "src/TimeOffManager.Application/"]
COPY ["src/TimeOffManager.Infrastructure/TimeOffManager.Infrastructure.csproj", "src/TimeOffManager.Infrastructure/"]
COPY ["src/TimeOffManager.Api/TimeOffManager.Api.csproj", "src/TimeOffManager.Api/"]
RUN dotnet restore "src/TimeOffManager.Api/TimeOffManager.Api.csproj"

# Copy the rest and publish.
COPY . .
RUN dotnet publish "src/TimeOffManager.Api/TimeOffManager.Api.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime stage (non-root) ────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app

ENTRYPOINT ["dotnet", "TimeOffManager.Api.dll"]
