using Biblioteca.Web.Models;
namespace Biblioteca.Web.Repositorios;

public interface ILibroRepositorio
{
    Task<IReadOnlyList<Libro>> ListarAsync(string? titulo);
    Task<Libro?> ObtenerAsync(int id);
    Task InsertarAsync(Libro libro);
    Task ActualizarAsync(Libro libro);
    Task EliminarAsync(int id);
}
public class LibroRepositorio(Conexion conexion) : Repositorio(conexion), ILibroRepositorio
{
    public Task<IReadOnlyList<Libro>> ListarAsync(string? titulo) => string.IsNullOrWhiteSpace(titulo)
     ? Consultar<Libro>("dbo.S08_Libros_Listar") : Consultar<Libro>("dbo.S08_Libros_Buscar", new { Titulo = titulo });
    public async Task<Libro?> ObtenerAsync(int id) => (await Consultar<Libro>("dbo.S08_Libros_Obtener", new { LibroId = id })).SingleOrDefault();
    public Task InsertarAsync(Libro l) => Ejecutar("dbo.S08_Libros_Insertar", new { l.Titulo, l.ISBN, l.AutorId, l.Ejemplares }, "ISBN");
    public Task ActualizarAsync(Libro l) => Ejecutar("dbo.S08_Libros_Actualizar", new { l.LibroId, l.Titulo, l.ISBN, l.AutorId, l.Ejemplares }, "ISBN");
    public Task EliminarAsync(int id) => Ejecutar("dbo.S08_Libros_Eliminar", new { LibroId = id });
}
