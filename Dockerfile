FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Install dotnet ef
RUN dotnet tool install --global dotnet-ef
# Restore as distinct layers
RUN dotnet restore
# Load development secrets
RUN cat /etc/secrets/appsettings.secrets.json | \
    dotnet user-secrets set --project FlyDreamAir/FlyDreamAir.csproj
# Build and publish a release
RUN dotnet publish --project FlyDreamAir/FlyDreamAir.csproj -c Release -o out
# Synchronize database state
RUN dotnet ef database update --project FlyDreamAir/FlyDreamAir.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "FlyDreamAir.dll"]
