# ========= ETAPA DE BUILD =========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copiar archivos de solución y proyectos
COPY Casa106.sln ./
COPY src/Casa106.Api/Casa106.Api.csproj src/Casa106.Api/
COPY src/Casa106.Application/Casa106.Application.csproj src/Casa106.Application/
COPY src/Casa106.Domain/Casa106.Domain.csproj src/Casa106.Domain/
COPY src/Casa106.Infrastructure/Casa106.Infrastructure.csproj src/Casa106.Infrastructure/

# Restaurar paquetes
RUN dotnet restore Casa106.sln

# Copiar el resto del código
COPY . .

# Publicar la API
RUN dotnet publish src/Casa106.Api/Casa106.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ========= ETAPA DE RUNTIME =========
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Casa106.Api.dll"]