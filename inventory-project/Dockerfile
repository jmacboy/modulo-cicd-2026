# Build context: inventory-project/
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so the package layer is cached until a .csproj changes.
COPY src/Inventory.Domain/Inventory.Domain.csproj src/Inventory.Domain/
COPY src/Inventory.Application/Inventory.Application.csproj src/Inventory.Application/
COPY src/Inventory.Infrastructure/Inventory.Infrastructure.csproj src/Inventory.Infrastructure/
COPY src/Inventory.WebApi/Inventory.WebApi.csproj src/Inventory.WebApi/
RUN dotnet restore src/Inventory.WebApi/Inventory.WebApi.csproj

COPY src/ src/
RUN dotnet publish src/Inventory.WebApi/Inventory.WebApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Inventory.WebApi.dll"]
