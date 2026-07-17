# ─── Etapa 1: build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# Copiar archivos de solución y proyectos primero para aprovechar caché de capas
COPY Casa106.sln .
COPY src/Casa106.Domain/Casa106.Domain.csproj          src/Casa106.Domain/
COPY src/Casa106.Application/Casa106.Application.csproj src/Casa106.Application/
COPY src/Casa106.Infrastructure/Casa106.Infrastructure.csproj src/Casa106.Infrastructure/
COPY src/Casa106.Api/Casa106.Api.csproj                src/Casa106.Api/

# Restaurar solo los proyectos .NET (excluir el esproj de React)
RUN dotnet restore Casa106.sln \
	--ignore-failed-sources

# Copiar el resto del código fuente
COPY src/Casa106.Domain/       src/Casa106.Domain/
COPY src/Casa106.Application/  src/Casa106.Application/
COPY src/Casa106.Infrastructure/ src/Casa106.Infrastructure/
COPY src/Casa106.Api/          src/Casa106.Api/

# Publicar en modo Release
RUN dotnet publish src/Casa106.Api/Casa106.Api.csproj \
	-c Release \
	-o /app/publish \
	--no-restore \
	/p:UseAppHost=false

# ─── Etapa 2: runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Crear directorio para uploads (almacenamiento local de documentos)
RUN mkdir -p /app/uploads

# Variables de entorno de producción
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Casa106.Api.dll"]
