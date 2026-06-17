namespace MicroServicioOrdenCompra.Config;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string OrdenesCollection { get; set; } = string.Empty;
}