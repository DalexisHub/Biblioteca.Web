using Biblioteca.Web.Models;
using Biblioteca.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
namespace Biblioteca.Web.Controllers;

public class PrestamosController(IPrestamoRepositorio prestamos) : Controller
{
    public async Task<IActionResult> Reporte(DateTime? desde, DateTime? hasta)
    {
        ViewData["Desde"] = Request.Query.ContainsKey("desde") ? Request.Query["desde"].ToString() : desde?.ToString("yyyy-MM-dd");
        ViewData["Hasta"] = Request.Query.ContainsKey("hasta") ? Request.Query["hasta"].ToString() : hasta?.ToString("yyyy-MM-dd");
        if (!Request.Query.ContainsKey("desde") && !Request.Query.ContainsKey("hasta")) return View(Array.Empty<PrestamoReporte>());
        if (desde is null) ModelState.AddModelError("desde", "Selecciona una fecha inicial válida.");
        if (hasta is null) ModelState.AddModelError("hasta", "Selecciona una fecha final válida.");
        if (desde > hasta) ModelState.AddModelError("hasta", "La fecha final debe ser igual o posterior a la inicial.");
        if (!ModelState.IsValid) return View(Array.Empty<PrestamoReporte>());
        ViewData["Consultado"] = true;
        return View(await prestamos.ReporteAsync(desde!.Value, hasta!.Value));
    }
}
