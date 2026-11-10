# Étape 1 : compilation
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/StageTrack.Api/StageTrack.Api.csproj src/StageTrack.Api/
RUN dotnet restore src/StageTrack.Api/StageTrack.Api.csproj
COPY src/ src/
RUN dotnet publish src/StageTrack.Api/StageTrack.Api.csproj -c Release -o /app --no-restore

# Étape 2 : image d'exécution légère
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080 \
    ConnectionStrings__Default="Data Source=/data/stagetrack.db"
VOLUME /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "StageTrack.Api.dll"]
