# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY *.sln .
COPY DistributedOrderSystem/*.csproj DistributedOrderSystem/
COPY DistributedOrderSystem.Domain/*.csproj DistributedOrderSystem.Domain/
COPY DistributedOrderSystem.Infrastructure/*.csproj DistributedOrderSystem.Infrastructure/

RUN dotnet restore

COPY . .
RUN dotnet publish DistributedOrderSystem/DistributedOrderSystem.csproj -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DistributedOrderSystem.dll"]
