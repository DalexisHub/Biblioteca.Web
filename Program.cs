using Biblioteca.Web.Repositorios;
if (args.Contains("--verificar")) { await Biblioteca.Web.Verificacion.Comprobaciones.EjecutarAsync(); return; }
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables().AddCommandLine(args);
builder.Services.AddControllersWithViews(options => options.Filters.Add<DatosExceptionFilter>());
builder.Services.AddSingleton<Conexion>();
builder.Services.AddScoped<ILibroRepositorio, LibroRepositorio>();
builder.Services.AddScoped<ISocioRepositorio, SocioRepositorio>();
builder.Services.AddScoped<IAutorRepositorio, AutorRepositorio>();
builder.Services.AddScoped<IPrestamoRepositorio, PrestamoRepositorio>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Libros}/{action=Index}/{id?}").WithStaticAssets();
app.Run();
