FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
EXPOSE 80
COPY --from=build-env /App/out .
# Asegúrate que la app escuche en 0.0.0.0
ENV ASPNETCORE_URLS=http://0.0.0.0:80 
ENTRYPOINT ["dotnet", "nscore.dll"]