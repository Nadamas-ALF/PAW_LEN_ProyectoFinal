using LEN_API.Datos;
using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Repositorios;
using System.Text;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var cadenaConexion = builder.Configuration.GetConnectionString("BD_LEN")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:BD_LEN. Ver el LEEME.txt de BaseDeDatos.");

// la fabrica sirve al registro de errores (conexion propia); el contexto con
// alcance de peticion sale de la misma fabrica para compartir opciones
builder.Services.AddDbContextFactory<ContextoLen>(opciones => opciones.UseSqlServer(cadenaConexion));
builder.Services.AddScoped(proveedor => proveedor.GetRequiredService<IDbContextFactory<ContextoLen>>().CreateDbContext());

var opcionesJwt = builder.Configuration.GetSection("Jwt").Get<OpcionesJwt>() ?? new OpcionesJwt();
opcionesJwt.Validar();
builder.Services.Configure<OpcionesJwt>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        // sin esto el handler renombra los claims y los nombres del contrato dejan de existir
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = opcionesJwt.Emisor,
            ValidateAudience = true,
            ValidAudience = opcionesJwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(opcionesJwt.Llave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "Correo",
            RoleClaimType = "NombreRol"
        };
    });

// acceso denegado por defecto: lo publico se marca a mano con [AllowAnonymous]
// Una sesion con contrasena temporal pendiente no pasa de aca: solo puede cambiarla.
// Esa regla vive en el API y no solo en la pantalla (HU-ACC-006)
var politicaNormal = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .RequireAssertion(contexto => contexto.User.FindFirst("TieneContrasenaTemporal")?.Value != "true")
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(politicaNormal)
    .SetFallbackPolicy(politicaNormal)
    .AddPolicy("CualquierSesion", politica => politica.RequireAuthenticatedUser());

builder.Services.Configure<OpcionesCorreo>(builder.Configuration.GetSection("Correo"));
builder.Services.Configure<OpcionesContrasenaTemporal>(builder.Configuration.GetSection("ContrasenaTemporal"));
builder.Services.AddSingleton<IServicioCorreo, ServicioCorreo>();

builder.Services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();
builder.Services.AddSingleton<IEmisorToken, EmisorToken>();
builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<IAccesoServicio, AccesoServicio>();
builder.Services.AddScoped<IUsuarioServicio, UsuarioServicio>();

builder.Services.AddSingleton<IRegistroErrores, RegistroErrores>();
builder.Services.AddScoped<IDiagnosticoRepositorio, DiagnosticoRepositorio>();
builder.Services.AddScoped<IDiagnosticoServicio, DiagnosticoServicio>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opciones =>
    {
        // un 400 de validacion tiene la misma forma que uno de negocio: { mensaje }
        opciones.InvalidModelStateResponseFactory = contexto =>
        {
            var mensaje = contexto.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
                ?? "Los datos enviados no son válidos.";
            return new BadRequestObjectResult(new RespuestaMensajeDto { Mensaje = mensaje });
        };
    });
builder.Services.AddOpenApi();

var app = builder.Build();

// Sin cuenta de envio solo se tolera en desarrollo: en otro entorno es un error de arranque
if (!app.Environment.IsDevelopment()
    && !app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpcionesCorreo>>().Value.EstaConfigurado)
{
    throw new InvalidOperationException("Falta configurar la seccion Correo. Ver CONTRATO_API.md, seccion 5.");
}

app.UseMiddleware<MiddlewareErrores>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
