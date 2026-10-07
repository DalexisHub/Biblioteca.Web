using Biblioteca.Web.Models;
using Biblioteca.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
namespace Biblioteca.Web.Controllers;

public class SociosController(ISocioRepositorio socios) : Controller
{
    public async Task<IActionResult> Index() => View(await socios.ListarAsync());
    public IActionResult Create() => View(new Socio());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("DNI,Nombre,Email")] Socio socio)
    {
        if (ModelState.IsValid)
        {
            try { await socios.InsertarAsync(socio); TempData["Exito"] = "El socio se registró correctamente."; return RedirectToAction(nameof(Index)); }
            catch (ReglaDatosException ex) { ModelState.AddModelError(ex.Campo, ex.Message); }
        }
        return View(socio);
    }
}
