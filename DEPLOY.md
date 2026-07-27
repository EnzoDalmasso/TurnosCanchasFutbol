# Deploy Turnos Canchas de Futbol

Arquitectura productiva actual:

- Frontend: Vercel.
- Backend: Render con Docker.
- Base de datos: PostgreSQL en Supabase.

## 1. Base de datos Supabase

Crear un proyecto en Supabase y copiar el connection string de PostgreSQL.

Formato esperado por el backend:

```txt
Host=HOST;Port=5432;Database=postgres;Username=USUARIO;Password=PASSWORD;SSL Mode=Require;Trust Server Certificate=true;
```

La variable que usa el backend es:

```txt
ConnectionStrings__WilsonDb
```

Aunque el producto ahora se llame `Turnos Canchas de Futbol`, ese nombre de variable se mantiene porque asi esta configurado internamente el backend.

## 2. Backend con Docker

El backend se publica usando el `Dockerfile` de la raiz.

Variables de entorno necesarias en Render:

```txt
ConnectionStrings__WilsonDb=connection-string-postgresql-supabase
SeguridadAdmin__ClaveInicial=clave-inicial-admin-segura
SeguridadAdmin__ClaveSoporte=clave-soporte-segura
Cors__OrigenesPermitidos__0=https://url-del-frontend-vercel
ASPNETCORE_ENVIRONMENT=Production
```

Reglas importantes:

- `SeguridadAdmin__ClaveInicial` debe tener al menos 12 caracteres.
- `SeguridadAdmin__ClaveSoporte` debe tener al menos 16 caracteres.
- `SeguridadAdmin__ClaveInicial` solo crea la primera credencial si la base todavia no tiene admin.
- `SeguridadAdmin__ClaveSoporte` permite resetear la contrasena del dueno desde soporte.
- `Cors__OrigenesPermitidos__0` debe ser exactamente la URL del frontend publicado.
- No guardar passwords ni connection strings dentro del repo.

## 3. Migraciones en produccion

Cuando la base productiva ya exista, aplicar migraciones desde una terminal local.

```powershell
$env:ConnectionStrings__WilsonDb="connection-string-postgresql-supabase"
dotnet ef database update --project "Wilson Futbol 5\Wilson Futbol 5.csproj" --startup-project "Wilson Futbol 5\Wilson Futbol 5.csproj"
```

Despues de aplicar migraciones, limpiar la variable local:

```powershell
Remove-Item Env:\ConnectionStrings__WilsonDb
```

## 4. Frontend en Vercel

Configurar el proyecto Vercel apuntando a la carpeta:

```txt
frontend
```

Variable de entorno:

```txt
VITE_API_URL=https://url-del-backend/api
```

Comando de build:

```txt
npm run build
```

Carpeta de salida:

```txt
dist
```

## 5. Seguridad antes de salir a vender

- Usar contrasena admin fuerte, no una de prueba.
- Usar clave de soporte larga y distinta a la contrasena admin.
- Rotar la clave de Supabase si alguna vez se pego por error en chats, capturas o commits.
- En Render, configurar todas las variables como Environment Variables.
- En Supabase, mantener SSL activado.
- En Supabase, revisar backups y politicas de acceso.
- En Vercel, configurar solo `VITE_API_URL`; no poner claves privadas en frontend.
- Confirmar que CORS permita solo el dominio real de Vercel.
- Probar que `/api/autenticacion-admin/login` responda 429 despues de varios intentos seguidos.
- Probar que `/WeatherForecast` ya no exista.

## 6. Prueba final

Probar en URL real:

- Consultar disponibilidad como cliente.
- Crear reserva.
- Entrar a `/admin`.
- Ver reserva pendiente.
- Ver turnos confirmados por fecha.
- Usar boton de WhatsApp manual.
- Confirmar reserva.
- Rechazar otra reserva.
- Crear turno fijo.
- Crear feriado o vacaciones.
- Crear y cancelar reserva especial.
- Modificar precio, sena, alias y mensaje de pago.
- Cambiar contrasena admin.
- Resetear contrasena con clave de soporte desde `.http`.
