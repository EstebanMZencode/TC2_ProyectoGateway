using Microsoft.AspNetCore.Mvc;
using MicroServicioCliente.Models;
using MicroServicioCliente.Services;

namespace MicroServicioCliente.Controllers;

[ApiController]
[Route("api/clientes")]
[Produces("application/json")]
public class ClientesController : ControllerBase
{
    private readonly ClienteService _clienteService;
    private readonly ILogger<ClientesController> _logger;

    public ClientesController(ClienteService clienteService, ILogger<ClientesController> logger)
    {
        _clienteService = clienteService;
        _logger = logger;
    }

    
    // Obtiene todos los clientes activos
    
    [HttpGet]
    [ProducesResponseType(typeof(List<ClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerTodos()
    {
        _logger.LogInformation("[GET] /api/clientes - Solicitud recibida");
        var clientes = await _clienteService.ObtenerTodosAsync();
        _logger.LogInformation("[GET] /api/clientes - Respondiendo con {Count} clientes", clientes.Count);
        return Ok(clientes);
    }

    
    // Obtiene un cliente por su ID de MongoDB
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObtenerPorId(string id)
    {
        _logger.LogInformation("[GET] /api/clientes/{Id} - Solicitud recibida", id);
        var cliente = await _clienteService.ObtenerPorIdAsync(id);

        if (cliente is null)
        {
            _logger.LogWarning("[GET] /api/clientes/{Id} - No encontrado", id);
            return NotFound(new { mensaje = $"No se encontró ningún cliente con ID: {id}" });
        }

        _logger.LogInformation("[GET] /api/clientes/{Id} - Respondiendo con cliente: {Nombre}", id, cliente.Nombre);
        return Ok(cliente);
    }

    
    // Obtiene un cliente por su número de cédula
    
    [HttpGet("cedula/{cedula}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorCedula(string cedula)
    {
        _logger.LogInformation("[GET] /api/clientes/cedula/{Cedula} - Solicitud recibida", cedula);
        var cliente = await _clienteService.ObtenerPorCedulaAsync(cedula);

        if (cliente is null)
            return NotFound(new { mensaje = $"No se encontró cliente con cédula: {cedula}" });

        return Ok(cliente);
    }

    
    // Endpoint de comunicación inter-servicio
    // Devuelve datos resumidos del cliente para que OrdenCompra los use.
    
    [HttpGet("{id}/resumen")]
    [ProducesResponseType(typeof(ClienteResumenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerResumen(string id)
    {
        _logger.LogInformation("[GET] /api/clientes/{Id}/resumen - Solicitud inter-servicio recibida", id);
        var resumen = await _clienteService.ObtenerResumenPorIdAsync(id);

        if (resumen is null)
        {
            _logger.LogWarning("[GET] /api/clientes/{Id}/resumen - Cliente no encontrado o inactivo", id);
            return NotFound(new { mensaje = $"Cliente no encontrado o inactivo: {id}" });
        }

        _logger.LogInformation("[GET] /api/clientes/{Id}/resumen - Respondiendo resumen para: {Nombre}", id, resumen.Nombre);
        return Ok(resumen);
    }

    
    // Registra un nuevo cliente.
    
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear([FromBody] CrearClienteDto dto)
    {
        _logger.LogInformation("[POST] /api/clientes - Solicitud de registro recibida para cédula: {Cedula}", dto.Cedula);

        if (string.IsNullOrWhiteSpace(dto.Cedula) || string.IsNullOrWhiteSpace(dto.Nombre) || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { mensaje = "Los campos Cedula, Nombre y Email son obligatorios." });

        var (cliente, error) = await _clienteService.CrearAsync(dto);

        if (error is not null)
        {
            _logger.LogWarning("[POST] /api/clientes - Conflicto: {Error}", error);
            return Conflict(new { mensaje = error });
        }

        _logger.LogInformation("[POST] /api/clientes - Cliente creado con ID: {Id}", cliente!.Id);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.Id }, cliente);
    }

    
    // Actualiza los datos de un cliente existente.
    
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Actualizar(string id, [FromBody] ActualizarClienteDto dto)
    {
        _logger.LogInformation("[PUT] /api/clientes/{Id} - Solicitud de actualización recibida", id);

        var (exito, error) = await _clienteService.ActualizarAsync(id, dto);

        if (error is not null)
        {
            _logger.LogWarning("[PUT] /api/clientes/{Id} - Error: {Error}", id, error);
            return error.Contains("no encontrado") ? NotFound(new { mensaje = error }) : BadRequest(new { mensaje = error });
        }

        if (!exito)
            return BadRequest(new { mensaje = "No se realizaron cambios." });

        _logger.LogInformation("[PUT] /api/clientes/{Id} - Actualizado correctamente", id);
        return NoContent();
    }

    
    // Elimina (baja lógica) un cliente por su ID.
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(string id)
    {
        _logger.LogInformation("[DELETE] /api/clientes/{Id} - Solicitud de baja recibida", id);

        var eliminado = await _clienteService.EliminarAsync(id);

        if (!eliminado)
        {
            _logger.LogWarning("[DELETE] /api/clientes/{Id} - Cliente no encontrado", id);
            return NotFound(new { mensaje = $"No se encontró cliente con ID: {id}" });
        }

        _logger.LogInformation("[DELETE] /api/clientes/{Id} - Baja lógica aplicada correctamente", id);
        return NoContent();
    }
}