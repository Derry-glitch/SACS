# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/SACS.API/SACS.API.csproj", "src/SACS.API/"]
COPY ["src/SACS.Domain/SACS.Domain.csproj", "src/SACS.Domain/"]
COPY ["src/SACS.Application/SACS.Application.csproj", "src/SACS.Application/"]
COPY ["src/SACS.Persistence/SACS.Persistence.csproj", "src/SACS.Persistence/"]
COPY ["src/SACS.Infrastructure/SACS.Infrastructure.csproj", "src/SACS.Infrastructure/"]
COPY ["src/SACS.BackgroundJobs/SACS.BackgroundJobs.csproj", "src/SACS.BackgroundJobs/"]

RUN dotnet restore "src/SACS.API/SACS.API.csproj"

# Copy the remaining files and build
COPY . .
WORKDIR "/src/src/SACS.API"
RUN dotnet build "SACS.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "SACS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SACS.API.dll"]
