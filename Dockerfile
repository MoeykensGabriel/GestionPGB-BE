# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY GestionPGB-BE.API/GestionPGB-BE.API.csproj GestionPGB-BE.API/
RUN dotnet restore GestionPGB-BE.API/GestionPGB-BE.API.csproj

COPY . .
RUN dotnet publish GestionPGB-BE.API/GestionPGB-BE.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Railway inyecta PORT; ASP.NET lo lee con ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

ENTRYPOINT ["dotnet", "GestionPGB-BE.API.dll"]
