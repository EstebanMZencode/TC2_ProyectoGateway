using MicroServicioCatalogo.Entities;

namespace MicroServicioCatalogo.Services
{
    public enum ResultadoOperacion
    {
        Exitoso,
        ValidacionFallida,
        NoEncontrado,
        Conflicto,
        ErrorConexion
    }

    public class ServiceResult<T>
    {
        public ResultadoOperacion Resultado { get; set; }
        public string? MensajeError { get; set; }
        public T? Datos { get; set; }

        public static ServiceResult<T> Exito(T datos) => new()
        {
            Resultado = ResultadoOperacion.Exitoso,
            Datos = datos
        };

        public static ServiceResult<T> Falla(ResultadoOperacion resultado, string mensajeError) => new()
        {
            Resultado = resultado,
            MensajeError = mensajeError
        };
    }

    public interface IProductoService
    {
        Task<ServiceResult<ProductoResponse>> CrearProductoAsync(ProductoCreateRequest request);
        Task<ServiceResult<PagedProductosResponse>> ObtenerProductosPaginadoAsync(int page, int size);
        Task<ServiceResult<ProductoResponse>> ObtenerProductoPorSkuAsync(string sku);
        Task<ServiceResult<bool>> ActualizarProductoAsync(string sku, ProductoUpdateRequest request);
        Task<ServiceResult<bool>> EliminarProductoAsync(string sku);
    }
}
