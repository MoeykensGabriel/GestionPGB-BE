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

# Railway inyecta PORT en runtime. Se usa entrypoint con shell para que ${PORT} se
# expanda al arrancar el contenedor (no en build) y Kestrel escuche en el puerto correcto.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet GestionPGB-BE.API.dll"]
