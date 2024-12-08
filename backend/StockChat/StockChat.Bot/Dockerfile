#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StockChat.Bot/StockChat.Bot.csproj", "StockChat.Bot/"]
COPY ["StockChat.API/StockChat.API.csproj", "StockChat.API/"]
COPY ["StockChat.Application/StockChat.Application.csproj", "StockChat.Application/"]
COPY ["StockChat.Domain/StockChat.Domain.csproj", "StockChat.Domain/"]
COPY ["StockChat.Infrastructure/StockChat.Infrastructure.csproj", "StockChat.Infrastructure/"]
RUN dotnet restore "./StockChat.Bot/StockChat.Bot.csproj"
COPY . .
WORKDIR "/src/StockChat.Bot"
RUN dotnet build "./StockChat.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./StockChat.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StockChat.Bot.dll"]