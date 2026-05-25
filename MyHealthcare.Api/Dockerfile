# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore (better layer caching)
COPY MyHealthcare.Shared/MyHealthcare.Shared.csproj MyHealthcare.Shared/
COPY MyHealthcare.Api/MyHealthcare.Api.csproj MyHealthcare.Api/
RUN dotnet restore MyHealthcare.Api/MyHealthcare.Api.csproj

# Copy everything else and build
COPY MyHealthcare.Shared/ MyHealthcare.Shared/
COPY MyHealthcare.Api/ MyHealthcare.Api/
RUN dotnet publish MyHealthcare.Api/MyHealthcare.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Render provides PORT env var; ASP.NET Core reads ASPNETCORE_URLS
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyHealthcare.Api.dll"]
