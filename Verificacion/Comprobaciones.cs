using System.ComponentModel.DataAnnotations;
using Biblioteca.Web.Controllers;
using Biblioteca.Web.Models;
using Biblioteca.Web.Repositorios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
namespace Biblioteca.Web.Verificacion;

// Pruebas de los contratos MVC sin acceder ni modificar BibliotecaDB.
public static class Comprobaciones
{
    public static async Task EjecutarAsync()
    {
        var count = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception("FALLÓ: " + name); Console.WriteLine("OK: " + name); count++; }
        var invalid = new Libro { Titulo = "", ISBN = new string('x', 21), AutorId = 0, Ejemplares = -1 };
        var errors = new List<ValidationResult>();
        Check(!Validator.TryValidateObject(invalid, new ValidationContext(invalid), errors, true) && errors.Count == 4, "Libro: título requerido, ISBN máximo, autor y ejemplares");
        var store = new Memoria();
        var controller = Contexto(new LibrosController(store, store));
        Check(await controller.Details(99) is NotFoundResult, "Details inexistente: 404");
        Check(await controller.Edit(99) is NotFoundResult, "Edit inexistente: 404");
        var good = new Libro { Titulo = "Libro de verificación", ISBN = "TEST", AutorId = 1, Ejemplares = 0 };
        Check(await controller.Create(good) is RedirectToActionResult && store.Insertados == 1 && controller.TempData.ContainsKey("Exito"), "Create: PRG y TempData");
        Check(await controller.Edit(2, new Libro { LibroId = 1 }) is BadRequestResult, "Edit rechaza ID manipulado");
        controller = Contexto(new LibrosController(store, store));
        Check(await controller.Create(new Libro { AutorId = 98 }) is ViewResult && store.Insertados == 1 && controller.ModelState.ContainsKey("AutorId"), "Create rechaza autor inactivo o inexistente");
        controller = Contexto(new LibrosController(store, store));
        controller.ModelState.AddModelError("Titulo", "Obligatorio");
        Check(await controller.Create(good) is ViewResult && store.Insertados == 1 && controller.ViewData["Autores"] is not null, "Create inválido conserva autores y no guarda");
        controller = Contexto(new LibrosController(store, store));
        good.LibroId = 1;
        Check(await controller.Edit(1, good) is RedirectToActionResult && store.Editados == 1, "Edit válido redirige y guarda");
        Check(await controller.Delete(1) is ViewResult && store.Bajas == 0, "Delete GET no cambia datos");
        Check(await controller.DeleteConfirmed(1) is RedirectToActionResult && store.Bajas == 1, "Delete POST aplica baja y PRG");
        var socios = Contexto(new SociosController(store));
        Check(await socios.Create(new Socio { DNI = "DUPLICADO" }) is ViewResult && socios.ModelState.ContainsKey("DNI"), "DNI duplicado: ModelState sin excepción");
        socios = Contexto(new SociosController(store));
        Check(await socios.Create(new Socio { DNI = "NUEVO" }) is RedirectToActionResult && socios.TempData.ContainsKey("Exito"), "Socio válido: PRG y TempData");
        var reporte = Contexto(new PrestamosController(store));
        Check(await reporte.Reporte(null, null) is ViewResult && store.Reportes == 0, "Reporte inicial no consulta datos");
        reporte.HttpContext.Request.QueryString = new QueryString("?desde=2026-10-02&hasta=2026-10-01");
        Check(await reporte.Reporte(new(2026, 10, 2), new(2026, 10, 1)) is ViewResult && !reporte.ModelState.IsValid && store.Reportes == 0, "Reporte rechaza rango invertido");
        reporte = Contexto(new PrestamosController(store));
        reporte.HttpContext.Request.QueryString = new QueryString("?desde=2026-10-01&hasta=2026-10-01");
        Check(await reporte.Reporte(new(2026, 10, 1), new(2026, 10, 1)) is ViewResult && store.Reportes == 1 && (string?)reporte.ViewData["Hasta"] == "2026-10-01", "Reporte acepta el mismo día y conserva filtros");
        Console.WriteLine($"{count} comprobaciones correctas. Repositorios simulados; no prueba SQL Server.");
    }
    static T Contexto<T>(T controller) where T : Controller
    {
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        controller.TempData = new TempDataDictionary(controller.HttpContext, new TempProvider());
        return controller;
    }
    sealed class TempProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
    sealed class Memoria : ILibroRepositorio, IAutorRepositorio, ISocioRepositorio, IPrestamoRepositorio
    {
        public int Insertados, Editados, Bajas, Reportes;
        public Task<IReadOnlyList<Libro>> ListarAsync(string? titulo) => Task.FromResult<IReadOnlyList<Libro>>([]);
        public Task<Libro?> ObtenerAsync(int id) => Task.FromResult<Libro?>(id == 1 ? new Libro { LibroId = 1, Titulo = "Verificación", AutorId = 1 } : null);
        public Task InsertarAsync(Libro libro) { Insertados++; return Task.CompletedTask; }
        public Task ActualizarAsync(Libro libro) { Editados++; return Task.CompletedTask; }
        public Task EliminarAsync(int id) { Bajas++; return Task.CompletedTask; }
        Task<IReadOnlyList<Autor>> IAutorRepositorio.ListarAsync() => Task.FromResult<IReadOnlyList<Autor>>([new Autor { AutorId = 1, Nombre = "Autor activo" }]);
        Task<IReadOnlyList<Socio>> ISocioRepositorio.ListarAsync() => Task.FromResult<IReadOnlyList<Socio>>([]);
        public Task InsertarAsync(Socio socio) => socio.DNI == "DUPLICADO" ? Task.FromException(new ReglaDatosException("DNI", "Ya existe ese DNI.")) : Task.CompletedTask;
        public Task<IReadOnlyList<PrestamoReporte>> ReporteAsync(DateTime desde, DateTime hasta) { Reportes++; return Task.FromResult<IReadOnlyList<PrestamoReporte>>([]); }
    }
}
