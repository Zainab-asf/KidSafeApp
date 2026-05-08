FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ../KidSafe.Shared/KidSafe.Shared.csproj ../KidSafe.Shared/
COPY KidSafe.Backend.csproj .
RUN dotnet restore

COPY ../KidSafe.Shared/ ../KidSafe.Shared/
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "KidSafe.Backend.dll"]
