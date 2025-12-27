# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and all project files
COPY . .

# Restore dependencies
RUN dotnet restore DistributedOrderSystem.slnx

# Publish the main project
RUN dotnet publish DistributedOrderSystem.csproj -c Release -o /app

# --- Runtime Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DistributedOrderSystem.dll"]
