FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OverlayApi/OverlayApi.csproj", "OverlayApi/"]
RUN dotnet restore "./OverlayApi/./OverlayApi.csproj"
COPY . .
WORKDIR "/src/OverlayApi"
RUN dotnet build "./OverlayApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OverlayApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER root
RUN apt-get update && apt-get install -y libfontconfig1

ENTRYPOINT ["dotnet", "OverlayApi.dll"]