# ── Build & test stage ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# Restore (cached layer)
COPY TaxCalculator.sln ./
COPY src/TaxCalculator.Models/TaxCalculator.Models.csproj           src/TaxCalculator.Models/
COPY src/TaxCalculator.Database/TaxCalculator.Database.csproj       src/TaxCalculator.Database/
COPY src/TaxCalculator.Service/TaxCalculator.Service.csproj         src/TaxCalculator.Service/
COPY src/TaxCalculator.API/TaxCalculator.API.csproj                 src/TaxCalculator.API/
COPY tests/TaxCalculator.Tests/TaxCalculator.Tests.csproj           tests/TaxCalculator.Tests/
RUN dotnet restore

# Copy source and run tests
COPY . .
RUN dotnet test tests/TaxCalculator.Tests/TaxCalculator.Tests.csproj \
    --configuration Release --no-restore --verbosity minimal

# Publish API
RUN dotnet publish src/TaxCalculator.API/TaxCalculator.API.csproj \
    --configuration Release --no-restore --output /app/publish

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaxCalculator.API.dll"]
