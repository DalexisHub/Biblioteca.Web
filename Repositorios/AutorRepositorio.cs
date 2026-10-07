using Biblioteca.Web.Models;
namespace Biblioteca.Web.Repositorios;

public interface IAutorRepositorio { Task<IReadOnlyList<Autor>> ListarAsync(); }
public class AutorRepositorio(Conexion conexion) : Repositorio(conexion), IAutorRepositorio
{
    public Task<IReadOnlyList<Autor>> ListarAsync() => Consultar<Autor>("dbo.S08_Autores_Listar");
}
