# 1. Baza s ASP.NET runtime-om
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# 2. SDK image za build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "./TerminoApp_NewBackend.csproj"
RUN dotnet build "./TerminoApp_NewBackend.csproj" -c Release -o /app/build
RUN dotnet publish "./TerminoApp_NewBackend.csproj" -c Release -o /app/publish

# 3. Završni image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TerminoApp_NewBackend.dll"]