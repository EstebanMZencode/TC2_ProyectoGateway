using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MicroServicioCliente.Models;

public class Cliente
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("cedula")]
    public string Cedula { get; set; } = string.Empty;

    [BsonElement("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("telefono")]
    public string Telefono { get; set; } = string.Empty;

    [BsonElement("direccion")]
    public Direccion Direccion { get; set; } = new();

    [BsonElement("activo")]
    public bool Activo { get; set; } = true;

    [BsonElement("fechaRegistro")]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}

public class Direccion
{
    [BsonElement("provincia")]
    public string Provincia { get; set; } = string.Empty;

    [BsonElement("canton")]
    public string Canton { get; set; } = string.Empty;

    [BsonElement("distrito")]
    public string Distrito { get; set; } = string.Empty;

    [BsonElement("detalle")]
    public string Detalle { get; set; } = string.Empty;
}