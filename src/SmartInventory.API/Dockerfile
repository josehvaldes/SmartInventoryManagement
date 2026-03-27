# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
RUN apk add --no-cache icu-libs
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SmartInventory.API/SmartInventory.API.csproj", "src/SmartInventory.API/"]
COPY ["src/SmartInventory.Application/SmartInventory.Application.csproj", "src/SmartInventory.Application/"]
COPY ["src/SmartInventory.Contracts/SmartInventory.Contracts.csproj", "src/SmartInventory.Contracts/"]
COPY ["src/SmartInventory.Domain/SmartInventory.Domain.csproj", "src/SmartInventory.Domain/"]
COPY ["src/SmartInventory.Infrastructure.AWS/SmartInventory.Infrastructure.AWS.csproj", "src/SmartInventory.Infrastructure.AWS/"]
COPY ["src/SmartInventory.Infrastructure/SmartInventory.Infrastructure.csproj", "src/SmartInventory.Infrastructure/"]
RUN dotnet restore "./src/SmartInventory.API/SmartInventory.API.csproj"
COPY . .
WORKDIR "/src/src/SmartInventory.API"
RUN dotnet build "./SmartInventory.API.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SmartInventory.API.csproj" -c $BUILD_CONFIGURATION --no-restore -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartInventory.API.dll"]