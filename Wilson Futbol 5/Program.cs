using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using Wilson_Futbol_5.Aplicacion.Interfaces;
using Wilson_Futbol_5.Aplicacion.Servicios;
using Wilson_Futbol_5.Infraestructura.Persistencia;
using Wilson_Futbol_5.Infraestructura.Seguridad;

namespace Wilson_Futbol_5
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ForwardedHeadersOptions>(opciones =>
            {
                opciones.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                opciones.ForwardLimit = 1;
                opciones.KnownIPNetworks.Clear();
                opciones.KnownProxies.Clear();
            });

            // Add services to the container.
            builder.Services.AddControllers();

            // Limitamos intentos sobre endpoints sensibles.
            // Esto ayuda contra fuerza bruta en login/reset y contra spam de reservas publicas.
            builder.Services.AddRateLimiter(opciones =>
            {
                opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                opciones.AddPolicy("AutenticacionAdmin", contexto =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        ObtenerClaveIp(contexto),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                opciones.AddPolicy("ReservasPublicas", contexto =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        ObtenerClaveIp(contexto),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 30,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));
            });

            builder.Services.AddScoped<IServicioTurnos, ServicioTurnos>();
            builder.Services.AddScoped<IServicioTurnosFijos, ServicioTurnosFijos>();
            builder.Services.AddScoped<IServicioExcepcionesHorario, ServicioExcepcionesHorario>();
            builder.Services.AddScoped<IServicioHorariosAtencion, ServicioHorariosAtencion>();
            builder.Services.AddScoped<IServicioConfiguracionNegocio, ServicioConfiguracionNegocio>();
            builder.Services.AddScoped<IServicioAutenticacionAdmin, ServicioAutenticacionAdmin>();

            // Al iniciar la app crea la primera contraseña del dueño si todavia no existe.
            builder.Services.AddHostedService<InicializadorCredencialAdmin>();

            // Leemos los origenes permitidos desde configuracion.
            // En local vienen de appsettings.Development.json y en produccion de variables de entorno.
            var origenesPermitidos = builder.Configuration
                .GetSection("Cors:OrigenesPermitidos")
                .Get<string[]>() ?? [];

            builder.Services.AddCors(opciones =>
            {
                opciones.AddPolicy("FrontendLocal", politica =>
                {
                    politica
                        .WithOrigins(origenesPermitidos)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Leemos la cadena de conexion que usara EF Core para conectarse a PostgreSQL/Supabase.
            var cadenaConexion = builder.Configuration.GetConnectionString("WilsonDb");

            if (string.IsNullOrWhiteSpace(cadenaConexion))
            {
                throw new InvalidOperationException("No se encontro la cadena de conexion 'WilsonDb'. Configurala en User Secrets o en las variables de entorno del hosting.");
            }

            // Registramos el DbContext con PostgreSQL para que EF Core pueda acceder a Supabase.
            builder.Services.AddDbContext<WilsonDbContext>(opciones =>
                opciones.UseNpgsql(cadenaConexion));

            var app = builder.Build();

            app.UseForwardedHeaders();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app.Use(async (contexto, siguiente) =>
            {
                contexto.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
                contexto.Response.Headers.TryAdd("X-Frame-Options", "DENY");
                contexto.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
                contexto.Response.Headers.TryAdd("Permissions-Policy", "geolocation=(), microphone=(), camera=()");

                await siguiente();
            });

            app.UseHttpsRedirection();

            app.UseCors("FrontendLocal");

            app.UseRateLimiter();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

        private static string ObtenerClaveIp(HttpContext contexto)
        {
            return contexto.Connection.RemoteIpAddress?.ToString() ?? "ip-desconocida";
        }
    }
}
