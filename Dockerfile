FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY Directory.Packages.props ./
COPY src/ src/
COPY api/service.proto api/service.proto
RUN dotnet publish src/WebAPI/RideShare.WebAPI.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080 8081
ENTRYPOINT ["dotnet", "RideShare.WebAPI.dll"]
