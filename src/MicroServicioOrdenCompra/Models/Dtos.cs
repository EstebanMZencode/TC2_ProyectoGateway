using System.ComponentModel.DataAnnotations;

namespace MicroServicioOrdenCompra.Models;

public class CrearOrdenDto
{
    [Required(ErrorMessage = "El ClienteId es obligatorio")]
    public string ClienteId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
    public string ClienteNombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email del cliente es obligatorio")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
    public string ClienteEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe incluir al menos un ítem")]
    [MinLength(1, ErrorMessage = "La orden debe tener al menos un ítem")]
    public List<CrearItemOrdenDto> Items { get; set; } = new();
}

public class CrearItemOrdenDto
{
    [Required(ErrorMessage = "El ProductoId es obligatorio")]
    public string ProductoId { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public int Cantidad { get; set; }
}

public class OrdenResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string NumeroOrden { get; set; } = string.Empty;
    public string ClienteId { get; set; } = string.Empty;
    public ClienteResumen ClienteResumen { get; set; } = new();
    public List<ItemOrden> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

public class ActualizarEstadoDto
{
    [Required]
    [RegularExpression("pendiente|pagada|enviada|entregada|cancelada",
        ErrorMessage = "Estado inválido. Use: pendiente, pagada, enviada, entregada o cancelada")]
    public string Estado { get; set; } = string.Empty;
}

public class ProductoDto
{
    public string Id { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}