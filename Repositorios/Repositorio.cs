using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
namespace Biblioteca.Web.Repositorios;

public abstract class Repositorio(Conexion conexion)
{
    protected async Task<IReadOnlyList<T>> Consultar<T>(string procedimiento, object? parametros = null)
    {
        using var db = conexion.Crear();
        return (await db.QueryAsync<T>(procedimiento, parametros, commandType: CommandType.StoredProcedure)).AsList();
    }
    protected async Task Ejecutar(string procedimiento, object parametros, string campoUnico = "")
    {
        using var db = conexion.Crear();
        try { await db.ExecuteAsync(procedimiento, parametros, commandType: CommandType.StoredProcedure); }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        { throw new ReglaDatosException(campoUnico, $"Ya existe un registro con ese {campoUnico}, incluso si está inactivo."); }
        catch (SqlException ex) when (ex.Number is 50001 or 50002 or 50003)
        { throw new ReglaDatosException(ex.Number == 50001 ? "AutorId" : "", ex.Message); }
    }
}
