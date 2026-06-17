using Microsoft.AspNetCore.Mvc;
using MicroServicioOrdenCompra.Models;
using MicroServicioOrdenCompra.Services;

namespace MicroServicioOrdenCompra.Controllers;

[ApiController]
[Route("api/ordenes")]
[Produces("application/json")]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenService _ordenService;
    private readonly ILogger<OrdenesController> _logger;

    public OrdenesController(IOrdenService ordenService, ILogger<OrdenesController> logger)
    {
        _ordenService = ordenService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        _logger.LogInformation("[OrdenesController] GET /api/ordenes recibido");
        var ordenes = await _ordenService.ObtenerTodasAsync();
        _logger.LogInformation("[OrdenesController] Respondiendo con {Count} órdenes", ordenes.Count);
        return Ok(new { success = true, data = ordenes, total = ordenes.Count });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(string id)
    {
        _logger.LogInformation("[OrdenesController] GET /api/ordenes/{Id} recibido", id);
        var orden = await _ordenService.ObtenerPorIdAsync(id);

        if (orden == null)
        {
            _logger.LogWarning("[OrdenesController] Orden {Id} no encontrada", id);
            return NotFound(new { success = false, message = $"Orden '{id}' no encontrada." });
        }

        _logger.LogInformation("[OrdenesController] Orden {Id} encontrada y devuelta", id);
        return Ok(new { success = true, data = orden });
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> ObtenerPorCliente(string clienteId)
    {
        _logger.LogInformation("[OrdenesController] GET /api/ordenes/cliente/{ClienteId} recibido", clienteId);
        var ordenes = await _ordenService.ObtenerPorClienteAsync(clienteId);
        _logger.LogInformation("[OrdenesController] {Count} órdenes encontradas para cliente {ClienteId}",
            ordenes.Count, clienteId);
        return Ok(new { success = true, data = ordenes, total = ordenes.Count });
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearOrdenDto dto)
    {
        _logger.LogInformation("[OrdenesController] POST /api/ordenes recibido para cliente: {ClienteId}",
            dto.ClienteId);

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var (orden, error) = await _ordenService.CrearOrdenAsync(dto);

        if (error != null)
        {
            _logger.LogWarning("[OrdenesController] Error al crear orden: {Error}", error);
            return BadRequest(new { success = false, message = error });
        }

        _logger.LogInformation("[OrdenesController] Orden creada. Número: {NumeroOrden}", orden!.NumeroOrden);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = orden.Id },
            new { success = true, data = orden });
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> ActualizarEstado(string id, [FromBody] ActualizarEstadoDto dto)
    {
        _logger.LogInformation("[OrdenesController] PATCH /api/ordenes/{Id}/estado → {Estado}", id, dto.Estado);

        if (!ModelState.IsValid)
            return BadRequest(new { success = false, errors = ModelState });

        var (exito, error) = await _ordenService.ActualizarEstadoAsync(id, dto.Estado);

        if (!exito)
            return error!.Contains("no encontrada")
                ? NotFound(new { success = false, message = error })
                : BadRequest(new { success = false, message = error });

        _logger.LogInformation("[OrdenesController] Estado de orden {Id} actualizado a {Estado}", id, dto.Estado);
        return Ok(new { success = true, message = $"Estado actualizado a '{dto.Estado}'." });
    }

    [HttpGet("{id}/historial")]
    public async Task<IActionResult> ObtenerHistorial(string id)
    {
        _logger.LogInformation("[OrdenesController] GET /api/ordenes/{Id}/historial recibido", id);
        var historial = await _ordenService.ObtenerHistorialEstadosAsync(id);
        return Ok(new { success = true, data = historial, total = historial.Count });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id)
    {
        _logger.LogInformation("[OrdenesController] DELETE /api/ordenes/{Id} recibido", id);
        var eliminado = await _ordenService.EliminarAsync(id);

        if (!eliminado)
            return NotFound(new { success = false, message = $"Orden '{id}' no encontrada." });

        _logger.LogInformation("[OrdenesController] Orden {Id} eliminada exitosamente", id);
        return Ok(new { success = true, message = "Orden eliminada." });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("[OrdenesController] Health check solicitado");
        return Ok(new
        {
            status = "healthy",
            service = "MicroServicioOrdenCompra",
            timestamp = DateTime.UtcNow
        });
    }
}