namespace MicroServicioCliente.Models;

// DTO para crear un nuevo cliente
public class CrearClienteDto
{
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DireccionDto Direccion { get; set; } = new();
}

// DTO para actualizar un cliente existente
public class ActualizarClienteDto
{
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public DireccionDto? Direccion { get; set; }
    public bool? Activo { get; set; }
}

// DTO para la dirección
public class DireccionDto
{
    public string Provincia { get; set; } = string.Empty;
    public string Canton { get; set; } = string.Empty;
    public string Distrito { get; set; } = string.Empty;
    public string Detalle { get; set; } = string.Empty;
}

// DTO de respuesta (lo que devuelve la API)
public class ClienteResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DireccionDto Direccion { get; set; } = new();
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }
}

// DTO simplificado para que OrdenCompra consulte datos del cliente
public class ClienteResumenDto
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
}