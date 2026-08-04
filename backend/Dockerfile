# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["backend/Schedulas.sln", "./"]
COPY ["backend/src/Schedulas.API/Schedulas.API.csproj", "src/Schedulas.API/"]
COPY ["backend/src/Schedulas.Application/Schedulas.Application.csproj", "src/Schedulas.Application/"]
COPY ["backend/src/Schedulas.Domain/Schedulas.Domain.csproj", "src/Schedulas.Domain/"]
COPY ["backend/src/Schedulas.Infrastructure/Schedulas.Infrastructure.csproj", "src/Schedulas.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/Schedulas.API/Schedulas.API.csproj"

# Copy the rest of the source code
COPY backend/ .

# Build the application
WORKDIR "/src/src/Schedulas.API"
RUN dotnet build "Schedulas.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Schedulas.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Production Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure environment variables
ENV ASPNETCORE_ENVIRONMENT=Production


ENTRYPOINT ["dotnet", "Schedulas.API.dll"]
