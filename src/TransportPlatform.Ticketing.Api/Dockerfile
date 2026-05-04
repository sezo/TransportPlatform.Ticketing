FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for layer caching
COPY ["src/TransportPlatform.Ticketing.Api/TransportPlatform.Ticketing.Api.csproj",            "src/TransportPlatform.Ticketing.Api/"]
COPY ["src/TransportPlatform.Ticketing.Infrastructure/TransportPlatform.Ticketing.Infrastructure.csproj", "src/TransportPlatform.Ticketing.Infrastructure/"]
COPY ["src/Ticketing.Application/Ticketing.Application.csproj",                                "src/Ticketing.Application/"]
COPY ["src/Ticketing.Domain/Ticketing.Domain.csproj",                                          "src/Ticketing.Domain/"]
COPY ["NuGet.config", "."]
RUN sed -i 's/localhost:5555/host.docker.internal:5555/g' NuGet.config

RUN dotnet restore "src/TransportPlatform.Ticketing.Api/TransportPlatform.Ticketing.Api.csproj"

COPY . .

WORKDIR "/src/src/TransportPlatform.Ticketing.Api"
RUN dotnet build "TransportPlatform.Ticketing.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TransportPlatform.Ticketing.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TransportPlatform.Ticketing.Api.dll"]
