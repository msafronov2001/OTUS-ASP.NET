FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj

RUN dotnet publish PromoCodeFactory.WebHost/PromoCodeFactory.WebHost.csproj \
    -c Release \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PromoCodeFactory.WebHost.dll"]
