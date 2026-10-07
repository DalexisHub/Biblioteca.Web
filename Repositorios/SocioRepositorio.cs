using Biblioteca.Web.Models;
namespace Biblioteca.Web.Repositorios;

public interface ISocioRepositorio
{
    Task<IReadOnlyList<Socio>> ListarAsync();
    Task InsertarAsync(Socio socio);
}
public class SocioRepositorio(Conexion conexion) : Repositorio(conexion), ISocioRepositorio
{
    public Task<IReadOnlyList<Socio>> ListarAsync() => Consultar<Socio>("dbo.S08_Socios_Listar");
    public Task InsertarAsync(Socio s) => Ejecutar("dbo.S08_Socios_Insertar", new { s.DNI, s.Nombre, s.Email }, "DNI");
}
