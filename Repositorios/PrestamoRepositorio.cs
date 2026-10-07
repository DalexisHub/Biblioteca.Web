using Biblioteca.Web.Models;
namespace Biblioteca.Web.Repositorios;

public interface IPrestamoRepositorio { Task<IReadOnlyList<PrestamoReporte>> ReporteAsync(DateTime desde, DateTime hasta); }
public class PrestamoRepositorio(Conexion conexion) : Repositorio(conexion), IPrestamoRepositorio
{
    public Task<IReadOnlyList<PrestamoReporte>> ReporteAsync(DateTime desde, DateTime hasta) => Consultar<PrestamoReporte>("dbo.S08_Prestamos_Reporte", new { Desde = desde.Date, Hasta = hasta.Date });
}
