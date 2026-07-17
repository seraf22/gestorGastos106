# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# Copy entire source structure
COPY . .

# Restore ALL packages from the SOLUTION (this ensures Application.Abstractions is available)
RUN dotnet restore "Casa106.sln"

# Build only the Api (NOT the entire solution to avoid compiling the React Web project)
# Api transitively compiles Domain → Application → Infrastructure
RUN dotnet build "src/Casa106.Api/Casa106.Api.csproj" -c Release --no-restore

# Publish only the Api
RUN dotnet publish "src/Casa106.Api/Casa106.Api.csproj" -c Release -o /app/publish --no-restore
# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
# Create upload directory
RUN mkdir -p /app/uploads
# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
# Copy published app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Casa106.Api.dll"]