using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        // el JWT viaja dentro de esta cookie, asi que JavaScript no lo puede leer (RQNF-SEC-004)
        opciones.Cookie.Name = "LEN.Sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        opciones.Cookie.SameSite = SameSiteMode.Strict;
        opciones.SlidingExpiration = false;
        opciones.LoginPath = "/Acceso/IniciarSesion";
        // un 403 de verdad: lo recoge UseStatusCodePagesWithReExecute y pinta la pantalla
        opciones.Events.OnRedirectToAccessDenied = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// acceso denegado por defecto: lo publico se marca a mano con [AllowAnonymous]
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IClienteApi, ClienteApi>(cliente =>
{
    var url = builder.Configuration["Valores:UrlAPI"]
        ?? throw new InvalidOperationException("Falta Valores:UrlAPI en la configuracion.");
    // sin la barra final, las rutas relativas pisarian el ultimo segmento
    cliente.BaseAddress = new Uri(url.EndsWith('/') ? url : url + "/");
    cliente.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Estado/{0}");

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
