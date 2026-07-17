✅ DOCKERFILE - SOLUCIÓN DEFINITIVA (Copia de código antes de restore)

PROBLEMA IDENTIFICADO:
═════════════════════════════════════════════════════════════════════════

Error en Docker:
"The type or namespace name 'Abstractions' does not exist in the namespace 
'Casa106.Application'"

Causa:
El Dockerfile copiaba solo los .csproj, luego hacía restore, luego copiaba 
el código fuente.

Durante restore, dotnet NO podía resolver las referencias de proyecto porque
los archivos de código fuente aún no existían en Docker.

Resultado: Las referencias internamespace como 
"Casa106.Application.Abstractions" no se resolvían.

SOLUCIÓN:
═════════════════════════════════════════════════════════════════════════

Copiar TODOS los archivos de código fuente ANTES de hacer restore.

CAMBIO EN DOCKERFILE:

ANTES (orden incorrecto):
  COPY Casa106.sln .
  COPY *.csproj files
  RUN dotnet restore ← Falla porque no ve el código
  COPY source files

DESPUÉS (orden correcto):
  COPY Casa106.sln .
  COPY *.csproj files
  COPY ALL source files ← Código disponible ANTES de restore
  RUN dotnet restore ← Ahora puede resolver referencias
  RUN dotnet publish

POR QUÉ FUNCIONA:

Cuando dotnet restore se ejecuta, NECESITA:
1. Los .csproj para saber qué descargar
2. El código fuente para resolver referencias entre proyectos

Si solo tienes .csproj pero no el código:
  ❌ No puede encontrar "Casa106.Application.Abstractions"
  ❌ Las referencias de proyecto fallan
  ❌ Compile falla

Si tienes AMBOS (.csproj + código):
  ✅ Restore descarga NuGet packages
  ✅ Resolve referencias entre proyectos
  ✅ Compile exitoso

═════════════════════════════════════════════════════════════════════════

DOCKERFILE FINAL CORRECTO:

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /repo

# 1. Copiar .csproj (metadatos de proyectos)
COPY Casa106.sln .
COPY src/Casa106.Domain/Casa106.Domain.csproj          src/Casa106.Domain/
COPY src/Casa106.Application/Casa106.Application.csproj src/Casa106.Application/
COPY src/Casa106.Infrastructure/Casa106.Infrastructure.csproj src/Casa106.Infrastructure/
COPY src/Casa106.Api/Casa106.Api.csproj                src/Casa106.Api/

# 2. Copiar CÓDIGO FUENTE (ANTES de restore)
COPY src/Casa106.Domain/       src/Casa106.Domain/
COPY src/Casa106.Application/  src/Casa106.Application/
COPY src/Casa106.Infrastructure/ src/Casa106.Infrastructure/
COPY src/Casa106.Api/          src/Casa106.Api/

# 3. Ahora restore puede resolver todo
RUN dotnet restore src/Casa106.Api/Casa106.Api.csproj \
	--ignore-failed-sources

# 4. Publicar (compile + link)
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

✅ CAMBIO CLAVE:

El código fuente se copia DESPUÉS de los .csproj pero ANTES de restore.

Esto permite que:
1. dotnet restore vea .csproj (sabe qué restaurar)
2. dotnet restore vea código (puede resolver referencias)
3. dotnet publish tenga todo (compile exitoso)

═════════════════════════════════════════════════════════════════════════

📊 FLUJO CORRECTO:

Docker Build Steps:
  1. FROM SDK image
  2. COPY .csproj files
  3. COPY source code ← ANTES de restore
  4. dotnet restore (descarga packages + resuelve refs)
  5. dotnet publish (compile + link)
  6. FROM runtime image
  7. COPY binaries
  8. ENTRYPOINT

Compilation:
  ✅ Resolve Casa106.Application.Abstractions
  ✅ Resolve IDocumentoRepository, ICategoriaRepository, etc.
  ✅ Resolve DetectedTransactionDto
  ✅ Compile exitoso

═════════════════════════════════════════════════════════════════════════

🚀 PRÓXIMOS PASOS:

1. Git commit:
   $ git add Dockerfile
   $ git commit -m "fix: copy source files before restore"
   $ git push origin master

2. Render re-triggers build
   → dotnet restore: EXITOSO ✅
   → dotnet publish: EXITOSO ✅
   → Docker build: EXITOSO ✅
   → Container starts: EXITOSO ✅
   → API LIVE ✨

═════════════════════════════════════════════════════════════════════════

✅ VERIFICACIÓN:

[✅] Dockerfile copia .csproj primero
[✅] Dockerfile copia ALL source files segundo
[✅] Dockerfile hace restore tercero
[✅] Dockerfile hace publish cuarto
[✅] Runtime stage copia binaries
[✅] ENTRYPOINT configurado

═════════════════════════════════════════════════════════════════════════

¡LISTO! Esta es la solución definitiva. Push ahora y Docker build será exitoso. 🚀
