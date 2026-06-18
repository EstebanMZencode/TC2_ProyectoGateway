using MicroServicioCatalogo.Entities;
using MicroServicioCatalogo.Services;

namespace MicroServicioCatalogo
{
    public static class ProductoEndpoints
    {
        public static void MapProductoEndpoints(this WebApplication app)
        {
            var grupo = app.MapGroup("/api/productos");

            grupo.MapPost("/", CrearProducto);
            grupo.MapGet("/", ObtenerProductosPaginado);
            grupo.MapGet("/{sku}", ObtenerProductoPorSku);
            grupo.MapPut("/{sku}", ActualizarProducto);
            grupo.MapDelete("/{sku}", EliminarProducto);
        }

        private static async Task<IResult> CrearProducto(ProductoCreateRequest request, IProductoService service, HttpRequest httpRequest)
        {
            var resultado = await service.CrearProductoAsync(request);

            if (resultado.Resultado != ResultadoOperacion.Exitoso)
            {
                return MapearError(resultado.Resultado, resultado.MensajeError!);
            }

            var ubicacion = $"{httpRequest.Scheme}://{httpRequest.Host}/api/productos/{resultado.Datos!.Sku}";
            return Results.Created(ubicacion, resultado.Datos);
        }

        private static async Task<IResult> ObtenerProductosPaginado(IProductoService service, int page = 1, int size = 10)
        {
            var resultado = await service.ObtenerProductosPaginadoAsync(page, size);

            if (resultado.Resultado != ResultadoOperacion.Exitoso)
            {
                return MapearError(resultado.Resultado, resultado.MensajeError!);
            }

            return Results.Ok(resultado.Datos);
        }

        private static async Task<IResult> ObtenerProductoPorSku(string sku, IProductoService service)
        {
            var resultado = await service.ObtenerProductoPorSkuAsync(sku);

            if (resultado.Resultado != ResultadoOperacion.Exitoso)
            {
                return MapearError(resultado.Resultado, resultado.MensajeError!);
            }

            return Results.Ok(resultado.Datos);
        }

        private static async Task<IResult> ActualizarProducto(string sku, ProductoUpdateRequest request, IProductoService service)
        {
            var resultado = await service.ActualizarProductoAsync(sku, request);

            if (resultado.Resultado != ResultadoOperacion.Exitoso)
            {
                return MapearError(resultado.Resultado, resultado.MensajeError!);
            }

            return Results.NoContent();
        }

        private static async Task<IResult> EliminarProducto(string sku, IProductoService service)
        {
            var resultado = await service.EliminarProductoAsync(sku);

            if (resultado.Resultado != ResultadoOperacion.Exitoso)
            {
                return MapearError(resultado.Resultado, resultado.MensajeError!);
            }

            return Results.NoContent();
        }

        private static IResult MapearError(ResultadoOperacion resultado, string mensaje)
        {
            return resultado switch
            {
                ResultadoOperacion.ValidacionFallida => Results.BadRequest(mensaje),
                ResultadoOperacion.NoEncontrado => Results.NotFound(mensaje),
                ResultadoOperacion.Conflicto => Results.Conflict(mensaje),
                ResultadoOperacion.ErrorConexion => Results.Json(mensaje, statusCode: StatusCodes.Status500InternalServerError),
                _ => Results.Json(mensaje, statusCode: StatusCodes.Status500InternalServerError)
            };
        }
    }
}

