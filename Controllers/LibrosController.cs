using Biblioteca.Web.Models;
using Biblioteca.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Biblioteca.Web.Controllers;

public class LibrosController(ILibroRepositorio libros, IAutorRepositorio autores) : Controller
{
    public async Task<IActionResult> Index(string? titulo)
    {
        ViewData["Busqueda"] = titulo;
        if (titulo?.Length > 200) { ModelState.AddModelError("titulo", "Busca un título de hasta 200 caracteres."); return View(Array.Empty<Libro>()); }
        return View(await libros.ListarAsync(titulo?.Trim()));
    }
    public async Task<IActionResult> Details(int id)
    { var libro = await libros.ObtenerAsync(id); return libro is null ? NotFound() : View(libro); }
    public async Task<IActionResult> Create()
    { await CargarAutores(); return View(new Libro()); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Titulo,ISBN,AutorId,Ejemplares")] Libro libro)
    {
        await ValidarAutor(libro);
        if (ModelState.IsValid)
        {
            try { await libros.InsertarAsync(libro); TempData["Exito"] = "El libro ya forma parte del catálogo."; return RedirectToAction(nameof(Index)); }
            catch (ReglaDatosException ex) { ModelState.AddModelError(ex.Campo, ex.Message); }
        }
        return View(libro);
    }
    public async Task<IActionResult> Edit(int id)
    { var libro = await libros.ObtenerAsync(id); if (libro is null) return NotFound(); await CargarAutores(); return View(libro); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("LibroId,Titulo,ISBN,AutorId,Ejemplares")] Libro libro)
    {
        if (id != libro.LibroId) return BadRequest();
        if (await libros.ObtenerAsync(id) is null) return NotFound();
        await ValidarAutor(libro);
        if (ModelState.IsValid)
        {
            try { await libros.ActualizarAsync(libro); TempData["Exito"] = "Los cambios del libro se guardaron."; return RedirectToAction(nameof(Index)); }
            catch (ReglaDatosException ex) { ModelState.AddModelError(ex.Campo, ex.Message); }
        }
        return View(libro);
    }
    public async Task<IActionResult> Delete(int id)
    { var libro = await libros.ObtenerAsync(id); return libro is null ? NotFound() : View(libro); }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var libro = await libros.ObtenerAsync(id); if (libro is null) return NotFound();
        if (ModelState.IsValid)
        {
            try { await libros.EliminarAsync(id); TempData["Exito"] = "El libro se dio de baja. Su historial se conserva."; return RedirectToAction(nameof(Index)); }
            catch (ReglaDatosException ex) { ModelState.AddModelError(ex.Campo, ex.Message); }
        }
        return View("Delete", libro);
    }
    private async Task CargarAutores() => ViewData["Autores"] = new SelectList(await autores.ListarAsync(), "AutorId", "Nombre");
    private async Task ValidarAutor(Libro libro)
    {
        var lista = await autores.ListarAsync();
        ViewData["Autores"] = new SelectList(lista, "AutorId", "Nombre");
        if (!lista.Any(a => a.AutorId == libro.AutorId)) ModelState.AddModelError("AutorId", "Selecciona un autor activo de la lista.");
    }
}
