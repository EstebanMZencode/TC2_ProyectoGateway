using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MicroServicioOrdenCompra.Models;

public class Orden
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("numeroOrden")]
    public string NumeroOrden { get; set; } = string.Empty;

    [BsonElement("clienteId")]
    public string ClienteId { get; set; } = string.Empty;

    [BsonElement("clienteResumen")]
    public ClienteResumen ClienteResumen { get; set; } = new();

    [BsonElement("items")]
    public List<ItemOrden> Items { get; set; } = new();

    [BsonElement("total")]
    public decimal Total { get; set; }

    [BsonElement("estado")]
    public string Estado { get; set; } = "pendiente";

    [BsonElement("fechaCreacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [BsonElement("fechaActualizacion")]
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}

public class ClienteResumen
{
    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;
}

public class ItemOrden
{
    [BsonElement("productoId")]
    public string ProductoId { get; set; } = string.Empty;

    [BsonElement("sku")]
    public string Sku { get; set; } = string.Empty;

    [BsonElement("nombreProducto")]
    public string NombreProducto { get; set; } = string.Empty;

    [BsonElement("cantidad")]
    public int Cantidad { get; set; }

    [BsonElement("precioUnitario")]
    public decimal PrecioUnitario { get; set; }

    [BsonElement("subtotal")]
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

public class EstadoOrden
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("ordenId")]
    public string OrdenId { get; set; } = string.Empty;

    [BsonElement("estadoAnterior")]
    public string EstadoAnterior { get; set; } = string.Empty;

    [BsonElement("estadoNuevo")]
    public string EstadoNuevo { get; set; } = string.Empty;

    [BsonElement("fechaCambio")]
    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

    [BsonElement("usuario")]
    public string Usuario { get; set; } = "sistema";
}