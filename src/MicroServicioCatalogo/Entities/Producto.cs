using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MicroServicioCatalogo.Entities
{
    public class Producto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("sku")]
        public string Sku { get; set; } = string.Empty;

        [BsonElement("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [BsonElement("descripcion")]
        public string? Descripcion { get; set; }

        [BsonElement("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [BsonElement("precio")]
        public decimal Precio { get; set; }

        [BsonElement("stock")]
        public int Stock { get; set; }

        [BsonElement("activo")]
        public bool Activo { get; set; }

        [BsonElement("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [BsonElement("fechaActualizacion")]
        public DateTime FechaActualizacion { get; set; }
    }
}
