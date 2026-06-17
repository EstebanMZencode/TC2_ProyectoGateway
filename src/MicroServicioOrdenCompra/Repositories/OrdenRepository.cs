using MongoDB.Driver;
using MicroServicioOrdenCompra.Config;
using MicroServicioOrdenCompra.Models;
using Microsoft.Extensions.Options;

namespace MicroServicioOrdenCompra.Repositories;

public interface IOrdenRepository
{
    Task<List<Orden>> ObtenerTodasAsync();
    Task<Orden?> ObtenerPorIdAsync(string id);
    Task<List<Orden>> ObtenerPorClienteAsync(string clienteId);
    Task<Orden> CrearAsync(Orden orden);
    Task<bool> ActualizarEstadoAsync(string id, string estadoAnterior, string nuevoEstado);
    Task<bool> EliminarAsync(string id);
    Task<List<EstadoOrden>> ObtenerHistorialEstadosAsync(string ordenId);
}

public class OrdenRepository : IOrdenRepository
{
    private readonly IMongoCollection<Orden> _ordenes;
    private readonly IMongoCollection<EstadoOrden> _estadosOrden;
    private readonly ILogger<OrdenRepository> _logger;

    public OrdenRepository(IOptions<MongoDbSettings> settings, ILogger<OrdenRepository> logger)
    {
        _logger = logger;
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _ordenes = database.GetCollection<Orden>(settings.Value.OrdenesCollection);
        _estadosOrden = database.GetCollection<EstadoOrden>("estados_orden");
    }

    public async Task<List<Orden>> ObtenerTodasAsync()
    {
        _logger.LogInformation("[OrdenRepository] Obteniendo todas las órdenes");
        return await _ordenes.Find(_ => true)
                             .SortByDescending(o => o.FechaCreacion)
                             .ToListAsync();
    }

    public async Task<Orden?> ObtenerPorIdAsync(string id)
    {
        _logger.LogInformation("[OrdenRepository] Buscando orden con ID: {Id}", id);
        return await _ordenes.Find(o => o.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<Orden>> ObtenerPorClienteAsync(string clienteId)
    {
        _logger.LogInformation("[OrdenRepository] Buscando órdenes del cliente: {ClienteId}", clienteId);
        return await _ordenes.Find(o => o.ClienteId == clienteId).ToListAsync();
    }

    public async Task<Orden> CrearAsync(Orden orden)
    {
        await _ordenes.InsertOneAsync(orden);
        _logger.LogInformation("[OrdenRepository] Orden creada con ID: {Id}, Número: {NumeroOrden}",
            orden.Id, orden.NumeroOrden);
        return orden;
    }

    public async Task<bool> ActualizarEstadoAsync(string id, string estadoAnterior, string nuevoEstado)
    {
        var update = Builders<Orden>.Update
            .Set(o => o.Estado, nuevoEstado)
            .Set(o => o.FechaActualizacion, DateTime.UtcNow);

        var result = await _ordenes.UpdateOneAsync(o => o.Id == id, update);

        if (result.ModifiedCount > 0)
        {
            // Guardar historial de cambio de estado
            var historial = new EstadoOrden
            {
                OrdenId = id,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = nuevoEstado,
                FechaCambio = DateTime.UtcNow,
                Usuario = "sistema"
            };
            await _estadosOrden.InsertOneAsync(historial);
            _logger.LogInformation("[OrdenRepository] Estado de orden {Id} cambiado: {Anterior} → {Nuevo}",
                id, estadoAnterior, nuevoEstado);
        }

        return result.ModifiedCount > 0;
    }

    public async Task<bool> EliminarAsync(string id)
    {
        var result = await _ordenes.DeleteOneAsync(o => o.Id == id);
        _logger.LogInformation("[OrdenRepository] Orden {Id} eliminada", id);
        return result.DeletedCount > 0;
    }

    public async Task<List<EstadoOrden>> ObtenerHistorialEstadosAsync(string ordenId)
    {
        _logger.LogInformation("[OrdenRepository] Obteniendo historial de estados para orden: {OrdenId}", ordenId);
        return await _estadosOrden.Find(e => e.OrdenId == ordenId)
                                  .SortByDescending(e => e.FechaCambio)
                                  .ToListAsync();
    }
}