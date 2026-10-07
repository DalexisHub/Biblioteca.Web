using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;
namespace Biblioteca.Web.Repositorios;

public class DatosExceptionFilter(ILogger<DatosExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not (DatosNoDisponiblesException or SqlException)) return;
        logger.LogError(context.Exception, "No se pudo completar el acceso a BibliotecaDB.");
        context.Result = new ViewResult
        {
            ViewName = "DatosNoDisponibles",
            StatusCode = 503,
            ViewData = new ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), context.ModelState)
        };
        context.ExceptionHandled = true;
    }
}
