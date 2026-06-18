using MicroServicioCatalogo.Entities;
using MicroServicioCatalogo.Repository;
using MongoDB.Driver;

namespace MicroServicioCatalogo.Services
{
    public class ProductoService : IProductoService
    {
        private readonly ProductoRepository _productoRepository;

        public ProductoService(ProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<ServiceResult<ProductoResponse>> CrearProductoAsync(ProductoCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku))
            {
                return ServiceResult<ProductoResponse>.Falla(ResultadoOperacion.ValidacionFallida, "El identificador es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return ServiceResult<ProductoResponse>.Falla(ResultadoOperacion.ValidacionFallida, "El nombre del producto es obligatorio");
            }

            try
            {
                var categoriaExiste = await _productoRepository.ExisteCategoriaAsync(request.Categoria);
                if (!categoriaExiste)
                {
                    return ServiceResult<ProductoResponse>.Falla(ResultadoOperacion.NoEncontrado, "La categoría especificada no existe.");
                }

                var productoExistente = await _productoRepository.ObtenerPorSkuAsync(request.Sku);
                if (productoExistente is not null)
                {
                    return ServiceResult<ProductoResponse>.Falla(ResultadoOperacion.Conflicto, "El producto ya existe en base de datos.");
                }

                var ahora = DateTime.UtcNow;
                var nuevoProducto = new Producto
                {
                    Sku = request.Sku,
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    Categoria = request.Categoria,
                    Precio = request.Precio,
                    Stock = request.Stock,
                    Activo = request.Activo,
                    FechaCreacion = ahora,
                    FechaActualizacion = ahora
                };

                await _productoRepository.CrearAsync(nuevoProducto);

                return ServiceResult<ProductoResponse>.Exito(ProductoResponse.FromEntity(nuevoProducto));
            }
            catch (MongoException)
            {
                return ServiceResult<ProductoResponse>.Falla(ResultadoOperacion.ErrorConexion, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }
        }

        public async Task<ServiceResult<PagedProductosResponse>> ObtenerProductosPaginadoAsync(int page, int size)
        {
            try
            {
                var (productos, totalRegistros) = await _productoRepository.ObtenerPaginadoAsync(page, size);

                if (productos.Count == 0)
                {
                    return ServiceResult<PagedProductosResponse>.Falla(ResultadoOperacion.NoEncontrado, "No se encontró ningún registro para productos.");
                }

                var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)size);

                var respuesta = new PagedProductosResponse
                {
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = page,
                        PageSize = size,
                        TotalRecords = totalRegistros,
                        TotalPages = totalPaginas,
                        HasNextPage = page < totalPaginas,
                        HasPreviousPage = page > 1
                    },
                    Data = productos.Select(ProductoResponse.FromEntity).ToList()
                };

                return ServiceResult<PagedProductosResponse>.Exito(respuesta);
            }
            catch (MongoException)
            {
                return ServiceResult<PagedProductosResponse>.Falla(ResultadoOperacion.ErrorConexion, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }
        }

        public async Task<ServiceResult<ProductoResponse>> ObtenerProductoPorSkuAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return ServiceResult<ProductoResponse>.Falla(ResultadoOperacion.ValidacionFallida, "El identificador es obligatorio");
            }

            try
            {
                var producto = await _productoRepository.ObtenerPorSkuAsync(sku);
                if (producto is null)
                {
                    return ServiceResult<ProductoResponse>.Falla(ResultadoOperacion.NoEncontrado, "No se encontró el producto solicitado.");
                }

                return ServiceResult<ProductoResponse>.Exito(ProductoResponse.FromEntity(producto));
            }
            catch (MongoException)
            {
                return ServiceResult<ProductoResponse>.Falla(ResultadoOperacion.ErrorConexion, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }
        }

        public async Task<ServiceResult<bool>> ActualizarProductoAsync(string sku, ProductoUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return ServiceResult<bool>.Falla(ResultadoOperacion.ValidacionFallida, "El identificador es obligatorio");
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return ServiceResult<bool>.Falla(ResultadoOperacion.ValidacionFallida, "El nombre del producto es obligatorio");
            }

            try
            {
                var productoExistente = await _productoRepository.ObtenerPorSkuAsync(sku);
                if (productoExistente is null)
                {
                    return ServiceResult<bool>.Falla(ResultadoOperacion.NoEncontrado, "No se encontró el identificador solicitado.");
                }

                var productoActualizado = new Producto
                {
                    Nombre = request.Nombre,
                    Descripcion = request.Descripcion,
                    Categoria = request.Categoria,
                    Precio = request.Precio,
                    Stock = request.Stock,
                    Activo = request.Activo,
                    FechaActualizacion = DateTime.UtcNow
                };

                await _productoRepository.ActualizarAsync(sku, productoActualizado);

                return ServiceResult<bool>.Exito(true);
            }
            catch (MongoException)
            {
                return ServiceResult<bool>.Falla(ResultadoOperacion.ErrorConexion, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }
        }

        public async Task<ServiceResult<bool>> EliminarProductoAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return ServiceResult<bool>.Falla(ResultadoOperacion.ValidacionFallida, "El identificador es obligatorio");
            }

            try
            {
                var eliminado = await _productoRepository.EliminarAsync(sku);
                if (!eliminado)
                {
                    return ServiceResult<bool>.Falla(ResultadoOperacion.NoEncontrado, "No se encontró el identificador solicitado.");
                }

                return ServiceResult<bool>.Exito(true);
            }
            catch (MongoException)
            {
                return ServiceResult<bool>.Falla(ResultadoOperacion.ErrorConexion, "No se pudo conectarse a la base de datos. Inténtelo más tarde.");
            }
        }
    }
}
