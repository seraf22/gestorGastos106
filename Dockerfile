# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# Copy entire source structure
COPY . .

# Restore ALL packages (this is critical - must also restore Application.Abstractions)
RUN dotnet restore "Casa106.sln"

# Build the entire SOLUTION (not just the Api project)
RUN dotnet build "Casa106.sln" -c Release --no-restore

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