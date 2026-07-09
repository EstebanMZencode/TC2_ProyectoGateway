using MicroServicioOrdenCompra.Models;
using MicroServicioOrdenCompra.Repositories;

namespace MicroServicioOrdenCompra.Services;

public interface IOrdenService
{
    Task<List<OrdenResponseDto>> ObtenerTodasAsync();
    Task<OrdenResponseDto?> ObtenerPorIdAsync(string id);
    Task<List<OrdenResponseDto>> ObtenerPorClienteAsync(string clienteId);
    Task<(OrdenResponseDto? orden, string? error)> CrearOrdenAsync(CrearOrdenDto dto);
    Task<(bool exito, string? error)> ActualizarEstadoAsync(string id, string nuevoEstado);
    Task<bool> EliminarAsync(string id);
    Task<List<EstadoOrden>> ObtenerHistorialEstadosAsync(string ordenId);
}

public class OrdenService : IOrdenService
{
    private readonly IOrdenRepository _repository;
    private readonly IProductoService _productoService;
    private readonly ILogger<OrdenService> _logger;

    // Contador para generar número de orden
    private static int _contadorOrden = 1;

    public OrdenService(IOrdenRepository repository, IProductoService productoService, ILogger<OrdenService> logger)
    {
        _repository = repository;
        _productoService = productoService;
        _logger = logger;
    }

    public async Task<List<OrdenResponseDto>> ObtenerTodasAsync()
    {
        _logger.LogInformation("[OrdenService] Solicitando todas las órdenes");
        var ordenes = await _repository.ObtenerTodasAsync();
        return ordenes.Select(MapToDto).ToList();
    }

    public async Task<OrdenResponseDto?> ObtenerPorIdAsync(string id)
    {
        _logger.LogInformation("[OrdenService] Buscando orden ID: {Id}", id);
        var orden = await _repository.ObtenerPorIdAsync(id);
        return orden == null ? null : MapToDto(orden);
    }

    public async Task<List<OrdenResponseDto>> ObtenerPorClienteAsync(string clienteId)
    {
        _logger.LogInformation("[OrdenService] Buscando órdenes del cliente: {ClienteId}", clienteId);
        var ordenes = await _repository.ObtenerPorClienteAsync(clienteId);
        return ordenes.Select(MapToDto).ToList();
    }

    public async Task<(OrdenResponseDto? orden, string? error)> CrearOrdenAsync(CrearOrdenDto dto)
    {
        _logger.LogInformation("[OrdenService] Creando orden para cliente: {ClienteId}", dto.ClienteId);

        var items = new List<ItemOrden>();
        decimal total = 0;

        foreach (var itemDto in dto.Items)
        {
            var producto = await _productoService.ObtenerProductoAsync(itemDto.ProductoId);

            if (producto == null)
                return (null, $"Producto '{itemDto.ProductoId}' no encontrado.");

            if (producto.Stock < itemDto.Cantidad)
                return (null, $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}");

            var item = new ItemOrden
            {
                ProductoId = producto.Id ?? producto.Sku,
                Sku = producto.Sku,
                NombreProducto = producto.Nombre,
                Cantidad = itemDto.Cantidad,
                PrecioUnitario = producto.Precio
            };

            items.Add(item);
            total += item.Subtotal;
        }

        // Generar número de orden único
        var numeroOrden = $"ORD-{DateTime.UtcNow.Year}-{_contadorOrden:D4}";
        _contadorOrden++;

        var orden = new Orden
        {
            NumeroOrden = numeroOrden,
            ClienteId = dto.ClienteId,
            ClienteResumen = new ClienteResumen
            {
                Nombre = dto.ClienteNombre,
                Email = dto.ClienteEmail
            },
            Items = items,
            Total = total,
            Estado = "pendiente",
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        var ordenCreada = await _repository.CrearAsync(orden);
        _logger.LogInformation("[OrdenService] Orden creada. Número: {NumeroOrden}, Total: {Total}",
            ordenCreada.NumeroOrden, ordenCreada.Total);

        return (MapToDto(ordenCreada), null);
    }

    public async Task<(bool exito, string? error)> ActualizarEstadoAsync(string id, string nuevoEstado)
    {
        var orden = await _repository.ObtenerPorIdAsync(id);
        if (orden == null)
            return (false, $"Orden '{id}' no encontrada.");

        var estadoAnterior = orden.Estado;
        var resultado = await _repository.ActualizarEstadoAsync(id, estadoAnterior, nuevoEstado);
        return resultado ? (true, null) : (false, "No se pudo actualizar el estado.");
    }

    public async Task<bool> EliminarAsync(string id)
    {
        return await _repository.EliminarAsync(id);
    }

    public async Task<List<EstadoOrden>> ObtenerHistorialEstadosAsync(string ordenId)
    {
        return await _repository.ObtenerHistorialEstadosAsync(ordenId);
    }

    private static OrdenResponseDto MapToDto(Orden orden) => new()
    {
        Id = orden.Id ?? string.Empty,
        NumeroOrden = orden.NumeroOrden,
        ClienteId = orden.ClienteId,
        ClienteResumen = orden.ClienteResumen,
        Items = orden.Items,
        Total = orden.Total,
        Estado = orden.Estado,
        FechaCreacion = orden.FechaCreacion,
        FechaActualizacion = orden.FechaActualizacion
    };
}