╔════════════════════════════════════════════════════════════════════════╗
║                                                                        ║
║              🚀 DEPLOY BACKEND API EN RENDER (10 minutos) 🚀          ║
║                                                                        ║
║  .NET 8 ASP.NET Core + Docker + Aiven PostgreSQL                      ║
║                                                                        ║
╚════════════════════════════════════════════════════════════════════════╝

ANTES DE EMPEZAR:
════════════════════════════════════════════════════════════════════════

✅ Frontend: GitHub Pages activo (https://seraf22.github.io/gestorGastos106)
✅ Dockerfile: Ya existe en la raíz del repo (listo para usar)
✅ appsettings.Production.json: Template creado
✅ Rama: master (GitHub detectará cambios)
✅ Base de datos: Aiven (ya configurada)

════════════════════════════════════════════════════════════════════════

📋 PASO 1: Preparar Credenciales Aiven (2 minutos)
════════════════════════════════════════════════════════════════════════

Necesitarás estos datos de tu base de datos Aiven:

URL: https://console.aiven.io
→ Busca tu servicio PostgreSQL
→ Copia estos datos:

  Host:       casa106-db-casa106.i.aivencloud.com
  Port:       16228
  Database:   defaultdb
  Username:   avnadmin
  Password:   [TU PASSWORD - cópialo bien]

⚠️ IMPORTANTE:
La contraseña NO debe compartirse públicamente.
En Render, marcamos como "Secret" y no aparece en logs.

════════════════════════════════════════════════════════════════════════

🌐 PASO 2: Ir a Render Dashboard (30 segundos)
════════════════════════════════════════════════════════════════════════

URL: https://dashboard.render.com

Login con GitHub:
→ Autoriza a Render acceder a tu GitHub
→ Selecciona el usuario: seraf22

════════════════════════════════════════════════════════════════════════

🔧 PASO 3: Crear Web Service (2 minutos)
════════════════════════════════════════════════════════════════════════

1. Click "New +" (top derecha de Render dashboard)
2. Selecciona "Web Service"
3. Conecta GitHub:
   → "Connect your GitHub account"
   → Busca: gestorGastos106
   → Click "Connect"

════════════════════════════════════════════════════════════════════════

⚙️ PASO 4: Configurar Web Service (3 minutos)
════════════════════════════════════════════════════════════════════════

En la página de configuración, llena:

NAME:
  casa106-api

BRANCH:
  master

ENVIRONMENT:
  Docker

(Render auto-detectará el Dockerfile y lo usará)

INSTANCE TYPE:
  Free (ó Starter si quieres mejor performance)

════════════════════════════════════════════════════════════════════════

🔐 PASO 5: Agregar Environment Variables (2 minutos)
════════════════════════════════════════════════════════════════════════

Scroll down hasta "Environment":

IMPORTANTE: Revisa que estos valores coincidan con tu Dockerfile y 
appsettings.Production.json

Agrega cada una presionando "+ Add Environment Variable":

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1️⃣  ASPNETCORE_ENVIRONMENT
	Key:   ASPNETCORE_ENVIRONMENT
	Value: Production
	Type:  Normal (no secret)

2️⃣  ASPNETCORE_URLS
	Key:   ASPNETCORE_URLS
	Value: http://+:8080
	Type:  Normal (no secret)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

3️⃣  DB_HOST
	Key:   DB_HOST
	Value: casa106-db-casa106.i.aivencloud.com
	Type:  Normal (no secret)

4️⃣  DB_PORT
	Key:   DB_PORT
	Value: 16228
	Type:  Normal (no secret)

5️⃣  DB_NAME
	Key:   DB_NAME
	Value: defaultdb
	Type:  Normal (no secret)

6️⃣  DB_USER
	Key:   DB_USER
	Value: avnadmin
	Type:  Normal (no secret)

7️⃣  DB_PASSWORD ⚠️ IMPORTANTE
	Key:   DB_PASSWORD
	Value: [Tu password de Aiven]
	Type:  🔒 Secret (MARCA COMO SECRET!)

	⚠️ AL MARCAR COMO SECRET:
	- No aparece en logs
	- No se ve en URL
	- No se expone en repo

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

💾 SAVE CONFIGURATION
	Click: "Create Web Service"

════════════════════════════════════════════════════════════════════════

🔨 PASO 6: Render Compila y Deploya (10 minutos, totalmente automático)
════════════════════════════════════════════════════════════════════════

Qué sucede automáticamente:

1. Render detecta el Dockerfile
2. Lee los pasos de build y runtime
3. Compila la imagen Docker (.NET 8)
4. Publica la aplicación
5. Inicia el contenedor
6. Inyecta las environment variables
7. La API comienza a escuchar en puerto 8080

Puedes ver el progreso en la sección "Events":
- "Building image..." → En progreso ⏳
- "Starting new server..." → Casi listo 🟡
- "Success" → ¡LISTO! 🟢

Tiempo total: ~10 minutos

════════════════════════════════════════════════════════════════════════

✅ PASO 7: Verificar que API está LIVE (2 minutos)
════════════════════════════════════════════════════════════════════════

Cuando el status sea verde "Live" ✅:

1. Tu API URL es:
   https://casa106-api.onrender.com

2. Prueba endpoints básicos:

   a) Swagger UI:
	  https://casa106-api.onrender.com/swagger
	  → Debes ver la documentación interactiva

   b) Health check:
	  curl https://casa106-api.onrender.com/health
	  → Debe responder algo (200 OK)

3. Si todo funciona, tu backend está LIVE ✨

════════════════════════════════════════════════════════════════════════

🔗 PASO 8: Conectar Frontend con Backend (1 minuto)
════════════════════════════════════════════════════════════════════════

Ahora tu frontend necesita saber la URL de la API.

Ve a GitHub y actualiza el secret:

URL: https://github.com/seraf22/gestorGastos106/settings/secrets/actions

Actualiza:
  Secret:  VITE_API_URL
  Value:   https://casa106-api.onrender.com

  (Antes decía: https://casa106-api.onrender.com - ahora es real ✅)

GitHub automáticamente:
→ Detecta cambio
→ Redeploya el frontend
→ Inyecta la URL real de la API
→ ¡Frontend + Backend conectados! 🔗

════════════════════════════════════════════════════════════════════════

🎯 VERIFICACIÓN FINAL (5 minutos)
════════════════════════════════════════════════════════════════════════

1. Frontend carga:
   https://seraf ←2.github.io/gestorGastos106/
   → Debe mostrar la interfaz React

2. API responde:
   https://casa106-api.onrender.com/swagger
   → Debe mostrar Swagger UI

3. Frontend llama API:
   - Abre DevTools (F12)
   - Network tab
   - Interactúa con la app
   - Verifica que ve requests a casa106-api.onrender.com
   - Response con datos de la BD

4. Base de datos conecta:
   - Las llamadas API traen datos de Aiven
   - Todo funciona sin errores

SI TODO ESTÁ VERDE ✅:
¡Tu app está COMPLETAMENTE EN PRODUCTION! 🎉

════════════════════════════════════════════════════════════════════════

⚠️ NOTAS IMPORTANTES:

1. Render Free Tier:
   - Puede pausarse si inactivo 15 minutos
   - Upgrade a Pro ($7/mes) para evitarlo
   - Plan Free es para desarrollo/demostración

2. Primera ejecución:
   - Build tarda ~10 minutos
   - Siguientes cambios: ~5 minutos
   - Render monitorea rama master automáticamente

3. Cambios futuros:
   - Si cambias Dockerfile: Render re-deploya
   - Si cambias env vars: Se aplican inmediato
   - Si cambias código .NET: Git commit + push → Auto re-deploy

4. Logs:
   - Render guarda logs por defecto
   - Si hay error, revisa la sección "Logs"
   - Problemas comunes: credenciales BD, puerto

════════════════════════════════════════════════════════════════════════

📊 ARQUITECTURA FINAL:

	GitHub (tu repo)
		 ↓
	Render monitorea master
		 ↓
	Dockerfile + env vars
		 ↓
	Docker build .NET 8
		 ↓
	Sube puerto 8080
		 ↓
	✨ API LIVE ✨
	https://casa106-api.onrender.com

	↓ conecta con ↓

	Aiven PostgreSQL
	casa106-db-casa106.i.aivencloud.com:16228

════════════════════════════════════════════════════════════════════════

🚀 TIMELINE TOTAL:

Paso 1:  Preparar Aiven           2 min
Paso 2:  Ir a Render              0.5 min
Paso 3:  Crear servicio           2 min
Paso 4:  Configurar               3 min
Paso 5:  Env variables            2 min
Paso 6:  Build automático         10 min
Paso 7:  Verificar                2 min
Paso 8:  Conectar frontend        1 min
─────────────────────────────────────
TOTAL:                             ~22.5 min

════════════════════════════════════════════════════════════════════════

✅ CHECKLIST ANTES DE EMPEZAR:

☐ Frontend está LIVE (GitHub Pages)
☐ Tienes credenciales Aiven a mano
☐ Dockerfile existe en raíz del repo
☐ Rama master está actualizada
☐ Casa106.Api compila localmente (dotnet build)

════════════════════════════════════════════════════════════════════════

LISTO PARA EMPEZAR:

1. Abre: https://dashboard.render.com
2. Sigue pasos 2-5
3. Espera 10 minutos (automático)
4. ¡Tu API estará LIVE! 🚀

════════════════════════════════════════════════════════════════════════

Preguntas:

Q: ¿Puedo cambiar la rama a algo que no sea master?
A: Sí, en Render especificas qué rama monitorear.

Q: ¿Qué pasa si se cae la API?
A: Render auto-reinicia. Logs te dicen por qué.

Q: ¿Cómo veo los errores?
A: En Render dashboard → Logs, todos los detalles.

Q: ¿Puedo desescalar después?
A: Sí, Render Free es gratis. Puedes pausar o borrar.

════════════════════════════════════════════════════════════════════════

¡A POR ELLO! 🎯

Ve a https://dashboard.render.com y comienza con el PASO 2.

Estarás LIVE en 20 minutos. ✨
