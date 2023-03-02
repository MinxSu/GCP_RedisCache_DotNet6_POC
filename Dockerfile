FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore
# Copy everything
COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
# Environment Variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "RedisConnection.dll"]

EXPOSE 8080