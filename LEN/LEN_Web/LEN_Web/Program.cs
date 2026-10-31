using LEN_Web.Utilidades;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
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

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
