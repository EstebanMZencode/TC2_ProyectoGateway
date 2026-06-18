namespace MicroServicioCatalogo.Repository
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ProductosCollectionName { get; set; } = string.Empty;
        public string CategoriasCollectionName { get; set; } = string.Empty;
    }
}
