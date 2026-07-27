# Turnos Canchas de Futbol

Aplicacion web para gestionar reservas de canchas de futbol. Incluye vista para clientes, panel del dueno, reservas pendientes de aprobacion, turnos confirmados por fecha, turnos fijos, feriados, vacaciones, reservas especiales, precios y datos de transferencia.

Repositorio:

```txt
https://github.com/EnzoDalmasso/TurnosCanchasFutbol
```

## Estructura

- `Wilson Futbol 5/`: backend ASP.NET Core con Entity Framework Core.
- `frontend/`: frontend React con Vite.
- `Dockerfile`: configuracion para publicar el backend como contenedor.
- `DEPLOY.md`: guia de despliegue.

> Nota: la carpeta local y el proyecto backend todavia conservan el nombre original `Wilson Futbol 5`. Eso no afecta al repo ni al funcionamiento.

## Desarrollo local

Backend:

```powershell
dotnet run --project "Wilson Futbol 5\Wilson Futbol 5.csproj"
```

Frontend:

```powershell
cd frontend
npm.cmd install
npm.cmd run dev
```

URL local habitual del frontend:

```txt
http://localhost:5173
```

URL local habitual del backend:

```txt
https://localhost:7094/api
```

## Variables del backend

En local, las claves sensibles pueden guardarse con User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:WilsonDb" "connection-string-local-o-supabase" --project "Wilson Futbol 5\Wilson Futbol 5.csproj"
dotnet user-secrets set "SeguridadAdmin:ClaveInicial" "clave-inicial-admin" --project "Wilson Futbol 5\Wilson Futbol 5.csproj"
dotnet user-secrets set "SeguridadAdmin:ClaveSoporte" "clave-soporte-segura" --project "Wilson Futbol 5\Wilson Futbol 5.csproj"
```

En produccion, configurar estas variables en el hosting del backend:

```txt
ConnectionStrings__WilsonDb=connection-string-produccion
SeguridadAdmin__ClaveInicial=clave-inicial-admin
SeguridadAdmin__ClaveSoporte=clave-soporte-segura
Cors__OrigenesPermitidos__0=https://url-del-frontend
```

Notas:

- `ConnectionStrings__WilsonDb` mantiene ese nombre porque asi esta configurado internamente el backend.
- `SeguridadAdmin__ClaveInicial` solo se usa si la base todavia no tiene credencial admin.
- `SeguridadAdmin__ClaveSoporte` permite resetear la contrasena del dueno desde soporte.
- `Cors__OrigenesPermitidos__0` debe coincidir con la URL real del frontend publicado.

## Variables del frontend

En `frontend/.env` para desarrollo local:

```env
VITE_API_URL=https://localhost:7094/api
```

En Vercel:

```env
VITE_API_URL=https://url-del-backend/api
```

## Base de datos

El backend usa PostgreSQL. En produccion se puede conectar a Supabase usando el connection string configurado en `ConnectionStrings__WilsonDb`.

Aplicar migraciones:

```powershell
dotnet ef database update --project "Wilson Futbol 5\Wilson Futbol 5.csproj" --startup-project "Wilson Futbol 5\Wilson Futbol 5.csproj"
```

## Docker backend

El backend puede publicarse como contenedor usando el `Dockerfile` de la raiz.

Build local:

```powershell
docker build -t turnos-canchas-futbol-api .
```

Run local:

```powershell
docker run --rm -p 8080:8080 `
  -e ConnectionStrings__WilsonDb="connection-string-produccion-o-prueba" `
  -e SeguridadAdmin__ClaveInicial="clave-inicial-admin" `
  -e SeguridadAdmin__ClaveSoporte="clave-soporte-segura" `
  -e Cors__OrigenesPermitidos__0="http://localhost:5173" `
  turnos-canchas-futbol-api
```

En Render u otro hosting con Docker, configurar las mismas variables de entorno.

## Checklist antes de publicar

- Configurar `VITE_API_URL` en Vercel.
- Configurar `ConnectionStrings__WilsonDb` en el hosting del backend.
- Configurar `SeguridadAdmin__ClaveInicial`.
- Configurar `SeguridadAdmin__ClaveSoporte`.
- Configurar `Cors__OrigenesPermitidos__0` con la URL de Vercel.
- Ejecutar migraciones en la base de produccion.
- Probar reserva cliente.
- Probar login admin.
- Probar confirmar/rechazar reserva.
- Probar turnos confirmados por fecha.
- Probar turnos fijos.
- Probar feriados/vacaciones.
- Probar reservas especiales.
