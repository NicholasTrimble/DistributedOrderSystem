using System.IO;

namespace DistributedOrderSystem.Tools
{
    public static class DockerfileGenerator
    {
        public static void GenerateDockerfile()
        {
            string dockerfileContent = """
# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY DistributedOrderSystem.csproj ./

# Restore dependencies
RUN dotnet restore

# Copy full source
COPY . .

# Publish release build
RUN dotnet publish -c Release -o /app

# --- Runtime Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DistributedOrderSystem.dll"]
""";

            File.WriteAllText("Dockerfile", dockerfileContent);
            Console.WriteLine("Dockerfile successfully generated!");
        }
    }
}
