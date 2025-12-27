# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution + project files
COPY DistributedOrderSystem.slnx ./
COPY DistributedOrderSystem/DistributedOrderSystem.csproj DistributedOrderSystem/
COPY DistributedOrderSystem.Domain/DistributedOrderSystem.Domain.csproj DistributedOrderSystem.Domain/
COPY DistributedOrderSystem.Infrastructure/DistributedOrderSystem.Infrastructure.csproj DistributedOrderSystem.Infrastructure/

# Restore dependencies
RUN dotnet restore DistributedOrderSystem.slnx

# Copy the rest of the repo
COPY . .

# Publish release build
RUN dotnet publish DistributedOrderSystem/DistributedOrderSystem.csproj -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DistributedOrderSystem.dll"]
