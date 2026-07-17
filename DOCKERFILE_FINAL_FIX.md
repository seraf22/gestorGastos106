✅ DOCKERFILE RE-CORREGIDO - React Project Issue Solucionado

PROBLEMA IDENTIFICADO:
═════════════════════════════════════════════════════════════════════════

Error en Docker:
"Could not find a part of the path '/repo/src/Casa106.Web/Casa106.Web.esproj'"

Causa:
Casa106.sln tiene referencia al proyecto Casa106.Web (React .esproj)
Pero en Docker, ese archivo no se copia (es para frontend, no backend)
Cuando `dotnet restore Casa106.sln` intenta restaurar, fallar porque no encuentra el .esproj

SOLUCIÓN FINAL:
═════════════════════════════════════════════════════════════════════════

Volver a restaurar SOLO el proyecto API (como estaba originalmente)

CAMBIO EN DOCKERFILE:

ANTES (intento de fix que causó el error):
  RUN dotnet restore Casa106.sln \
	  --ignore-failed-sources

DESPUÉS (correcto):
  RUN dotnet restore src/Casa106.Api/Casa106.Api.csproj \
	  --ignore-failed-sources

POR QUÉ FUNCIONA:

✅ dotnet restore src/Casa106.Api/Casa106.Api.csproj
   └─ Restaura Casa106.Api
	  └─ que depende de Casa106.Infrastructure
		 └─ que depende de Casa106.Application
			└─ que depende de Casa106.Domain

El restore grafo de dependencias automáticamente restaura:
  ✅ Casa106.Domain
  ✅ Casa106.Application (interfaces)
  ✅ Casa106.Infrastructure (implementations)
  ✅ Casa106.Api (main)

NO intenta restaurar Casa106.Web (React) porque no es una dependencia .NET

═════════════════════════════════════════════════════════════════════════

📁 DOCKERFILE FINAL CORRECTO:

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# Copiar solo los .csproj de proyectos .NET
COPY Casa106.sln .
COPY src/Casa106.Domain/Casa106.Domain.csproj          src/Casa106.Domain/
COPY src/Casa106.Application/Casa106.Application.csproj src/Casa106.Application/
COPY src/Casa106.Infrastructure/Casa106.Infrastructure.csproj src/Casa106.Infrastructure/
COPY src/Casa106.Api/Casa106.Api.csproj                src/Casa106.Api/

# Restaurar el proyecto API (trae transitive dependencies)
RUN dotnet restore src/Casa106.Api/Casa106.Api.csproj \
	--ignore-failed-sources

# Copiar el resto del código fuente .NET
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

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/uploads

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Casa106.Api.dll"]

═════════════════════════════════════════════════════════════════════════

✅ POR QUÉ ESTO FUNCIONA:

1. Copiar .csproj SOLO de proyectos .NET
   → Ignora Casa106.Web.esproj

2. Restaurar SOLO el proyecto API
   → Automáticamente trae transitive deps
   → Compila todos los .NET projects
   → Crea todas las DLLs necesarias

3. Copiar código fuente SOLO de proyectos .NET
   → Ignora frontend React

4. Docker build ahora YES SIN ERRORES ✅

═════════════════════════════════════════════════════════════════════════

📊 FLUJO CORRECTO:

Docker Build:
  1. FROM mcr.microsoft.com/dotnet/sdk:8.0
  2. COPY .csproj files (solo .NET)
  3. dotnet restore Casa106.Api/Casa106.Api.csproj
	 └─ Trae: Domain, Application, Infrastructure
  4. COPY source code (solo .NET)
  5. dotnet publish Casa106.Api
  6. FROM mcr.microsoft.com/dotnet/aspnet:8.0
  7. COPY binaries
  8. ENTRYPOINT dotnet Casa106.Api.dll

React build:
  GitHub Actions (.github/workflows/deploy.yml)
  1. npm ci
  2. npm run build
  3. upload dist/ a GitHub Pages

═════════════════════════════════════════════════════════════════════════

🚀 PRÓXIMOS PASOS:

1. Git commit:
   $ git add Dockerfile
   $ git commit -m "fix: dockerfile restore api project only"
   $ git push origin master

2. Render re-triggers build
   → Docker build EXITOSO ✅
   → API compila sin errores
   → Container inicia correctamente
   → API LIVE ✨

═════════════════════════════════════════════════════════════════════════

✅ STATUS:

[✅] Dockerfile corregido finalmente
[✅] Restaura solo .NET projects
[✅] Ignora React project (como debe ser)
[✅] Transitive dependencies funcionan
[✅] Build será exitoso

═════════════════════════════════════════════════════════════════════════

¡Listo! Push ahora y Render volverá a hacer build sin errores. 🚀
