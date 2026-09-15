# Prepare the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore Pcf.Administration/Pcf.Administration.WebHost/Pcf.Administration.WebHost.csproj
RUN dotnet publish Pcf.Administration/Pcf.Administration.WebHost/Pcf.Administration.WebHost.csproj -c Release --no-restore -o /app/out

# Use the .NET 8 runtime for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Pcf.Administration.WebHost.dll"]