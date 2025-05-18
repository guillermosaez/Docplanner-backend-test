FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SlotManager.API/SlotManager.API.csproj", "SlotManager/SlotManager.API/"]
COPY ["SlotManager.Application/SlotManager.Application.csproj", "SlotManager/SlotManager.Application/"]
COPY ["SlotManager.Domain/SlotManager.Domain.csproj", "SlotManager/SlotManager.Domain/"]
COPY ["SlotManager.Infrastructure/SlotManager.Infrastructure.csproj", "SlotManager/SlotManager.Infrastructure/"]
COPY ["SlotManager.Infrastructure/SlotManager.EventConsumers.csproj", "SlotManager/SlotManager.EventConsumers/"]
RUN dotnet restore "SlotManager/SlotManager.API/SlotManager.API.csproj"
WORKDIR "/src/SlotManager"
COPY . .
WORKDIR "/src/SlotManager/SlotManager.API"
RUN dotnet build "SlotManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SlotManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SlotManager.API.dll"]
