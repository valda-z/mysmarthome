FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["MySmartHomeCore/MySmartHomeCore.csproj", "MySmartHomeCore/"]
RUN dotnet restore "MySmartHomeCore/MySmartHomeCore.csproj"
COPY . .
WORKDIR "/src/MySmartHomeCore"
RUN dotnet build "MySmartHomeCore.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MySmartHomeCore.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN rm appsettings.*
ENTRYPOINT ["dotnet", "MySmartHomeCore.dll"]