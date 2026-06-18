using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MicroServicioCliente.Configuration;
using MicroServicioCliente.Models;

namespace MicroServicioCliente.Services;

public class ClienteService
{
    private readonly IMongoCollection<Cliente> _clientes;
    private readonly ILogger<ClienteService> _logger;

    public ClienteService(IOptions<MongoDbSettings> settings, ILogger<ClienteService> logger)
    {
        _logger = logger;

        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        var database = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _clientes = database.GetCollection<Cliente>(settings.Value.ClientesCollection);

        // Índice único en cédula para evitar duplicados
        var indexKeyCedula = Builders<Cliente>.IndexKeys.Ascending(c => c.Cedula);
        var indexOptsCedula = new CreateIndexOptions { Unique = true };
        _clientes.Indexes.CreateOne(new CreateIndexModel<Cliente>(indexKeyCedula, indexOptsCedula));

        // Índice en email (no único, pero útil para búsquedas)
        var indexKeyEmail = Builders<Cliente>.IndexKeys.Ascending(c => c.Email);
        _clientes.Indexes.CreateOne(new CreateIndexModel<Cliente>(indexKeyEmail));

        _logger.LogInformation("ClienteService inicializado. Conectado a MongoDB.");
    }

    // GET: Obtener todos los clientes activos
    public async Task<List<ClienteResponseDto>> ObtenerTodosAsync()
    {
        _logger.LogInformation("Consultando todos los clientes activos...");
        var clientes = await _clientes
            .Find(c => c.Activo == true)
            .SortByDescending(c => c.FechaRegistro)
            .ToListAsync();

        _logger.LogInformation("Se encontraron {Count} clientes activos.", clientes.Count);
        return clientes.Select(MapToResponseDto).ToList();
    }

    // GET: Obtener cliente por ID
    public async Task<ClienteResponseDto?> ObtenerPorIdAsync(string id)
    {
        _logger.LogInformation("Buscando cliente con ID: {Id}", id);

        if (!ObjectId.TryParse(id, out _))
        {
            _logger.LogWarning("ID inválido: {Id}", id);
            return null;
        }

        var cliente = await _clientes.Find(c => c.Id == id).FirstOrDefaultAsync();

        if (cliente is null)
            _logger.LogWarning("Cliente no encontrado con ID: {Id}", id);
        else
            _logger.LogInformation("Cliente encontrado: {Nombre}", cliente.Nombre);

        return cliente is null ? null : MapToResponseDto(cliente);
    }

    // GET: Obtener cliente por cédula (usado por otros microservicios)
    public async Task<ClienteResponseDto?> ObtenerPorCedulaAsync(string cedula)
    {
        _logger.LogInformation("Buscando cliente con cédula: {Cedula}", cedula);
        var cliente = await _clientes.Find(c => c.Cedula == cedula && c.Activo == true).FirstOrDefaultAsync();
        return cliente is null ? null : MapToResponseDto(cliente);
    }

    // GET: Resumen del cliente para comunicación inter-servicio (OrdenCompra)
    public async Task<ClienteResumenDto?> ObtenerResumenPorIdAsync(string id)
    {
        _logger.LogInformation("OrdenCompra solicita resumen de cliente ID: {Id}", id);

        if (!ObjectId.TryParse(id, out _)) return null;

        var cliente = await _clientes
            .Find(c => c.Id == id && c.Activo == true)
            .Project(c => new ClienteResumenDto
            {
                Id = c.Id!,
                Nombre = c.Nombre,
                Email = c.Email,
                Cedula = c.Cedula
            })
            .FirstOrDefaultAsync();

        return cliente;
    }

    // POST: Registrar nuevo cliente
    public async Task<(ClienteResponseDto? cliente, string? error)> CrearAsync(CrearClienteDto dto)
    {
        _logger.LogInformation("Registrando nuevo cliente con cédula: {Cedula}", dto.Cedula);

        // Verificar que la cédula no exista ya
        var existe = await _clientes.Find(c => c.Cedula == dto.Cedula).AnyAsync();
        if (existe)
        {
            _logger.LogWarning("Ya existe un cliente con cédula: {Cedula}", dto.Cedula);
            return (null, $"Ya existe un cliente registrado con la cédula {dto.Cedula}.");
        }

        var nuevoCliente = new Cliente
        {
            Cedula = dto.Cedula.Trim(),
            Nombre = dto.Nombre.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Telefono = dto.Telefono.Trim(),
            Direccion = new Direccion
            {
                Provincia = dto.Direccion.Provincia,
                Canton = dto.Direccion.Canton,
                Distrito = dto.Direccion.Distrito,
                Detalle = dto.Direccion.Detalle
            },
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        await _clientes.InsertOneAsync(nuevoCliente);
        _logger.LogInformation("Cliente creado exitosamente. ID: {Id}", nuevoCliente.Id);

        return (MapToResponseDto(nuevoCliente), null);
    }

    // PUT: Actualizar datos del cliente
    public async Task<(bool exito, string? error)> ActualizarAsync(string id, ActualizarClienteDto dto)
    {
        _logger.LogInformation("Actualizando cliente ID: {Id}", id);

        if (!ObjectId.TryParse(id, out _))
            return (false, "ID de cliente inválido.");

        var cliente = await _clientes.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (cliente is null)
            return (false, "Cliente no encontrado.");

        var update = Builders<Cliente>.Update.Combine();
        var updates = new List<UpdateDefinition<Cliente>>();

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            updates.Add(Builders<Cliente>.Update.Set(c => c.Nombre, dto.Nombre.Trim()));

        if (!string.IsNullOrWhiteSpace(dto.Email))
            updates.Add(Builders<Cliente>.Update.Set(c => c.Email, dto.Email.Trim().ToLower()));

        if (!string.IsNullOrWhiteSpace(dto.Telefono))
            updates.Add(Builders<Cliente>.Update.Set(c => c.Telefono, dto.Telefono.Trim()));

        if (dto.Direccion is not null)
            updates.Add(Builders<Cliente>.Update.Set(c => c.Direccion, new Direccion
            {
                Provincia = dto.Direccion.Provincia,
                Canton = dto.Direccion.Canton,
                Distrito = dto.Direccion.Distrito,
                Detalle = dto.Direccion.Detalle
            }));

        if (dto.Activo.HasValue)
            updates.Add(Builders<Cliente>.Update.Set(c => c.Activo, dto.Activo.Value));

        if (updates.Count == 0)
            return (false, "No se enviaron campos para actualizar.");

        var combinedUpdate = Builders<Cliente>.Update.Combine(updates);
        var result = await _clientes.UpdateOneAsync(c => c.Id == id, combinedUpdate);

        if (result.ModifiedCount > 0)
            _logger.LogInformation("Cliente ID {Id} actualizado correctamente.", id);

        return (result.ModifiedCount > 0, null);
    }

    // DELETE: Baja lógica (soft delete)
    public async Task<bool> EliminarAsync(string id)
    {
        _logger.LogInformation("Eliminando (baja lógica) cliente ID: {Id}", id);

        if (!ObjectId.TryParse(id, out _)) return false;

        var update = Builders<Cliente>.Update.Set(c => c.Activo, false);
        var result = await _clientes.UpdateOneAsync(c => c.Id == id, update);

        if (result.ModifiedCount > 0)
            _logger.LogInformation("Cliente ID {Id} desactivado (soft delete).", id);

        return result.ModifiedCount > 0;
    }

    // Mapper interno: Cliente (MongoDB) → ClienteResponseDto
    private static ClienteResponseDto MapToResponseDto(Cliente c) => new()
    {
        Id = c.Id!,
        Cedula = c.Cedula,
        Nombre = c.Nombre,
        Email = c.Email,
        Telefono = c.Telefono,
        Direccion = new DireccionDto
        {
            Provincia = c.Direccion.Provincia,
            Canton = c.Direccion.Canton,
            Distrito = c.Direccion.Distrito,
            Detalle = c.Direccion.Detalle
        },
        Activo = c.Activo,
        FechaRegistro = c.FechaRegistro
    };
}