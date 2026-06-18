using MicroServicioCatalogo.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MicroServicioCatalogo.Repository
{
    public class ProductoRepository
    {
        private readonly IMongoCollection<Producto> _productosCollection;
        private readonly IMongoCollection<Categoria> _categoriasCollection;

        public ProductoRepository(IDbConnectionFactory connectionFactory, IOptions<MongoDbSettings> settings)
        {
            _productosCollection = connectionFactory.GetCollection<Producto>(settings.Value.ProductosCollectionName);
            _categoriasCollection = connectionFactory.GetCollection<Categoria>(settings.Value.CategoriasCollectionName);
        }

        public async Task<Producto?> ObtenerPorSkuAsync(string sku)
        {
            var filtro = Builders<Producto>.Filter.Eq(p => p.Sku, sku);
            return await _productosCollection.Find(filtro).FirstOrDefaultAsync();
        }

        public async Task<(List<Producto> Productos, long TotalRegistros)> ObtenerPaginadoAsync(int page, int size)
        {
            var totalRegistros = await _productosCollection.CountDocumentsAsync(Builders<Producto>.Filter.Empty);

            var productos = await _productosCollection
                .Find(Builders<Producto>.Filter.Empty)
                .SortBy(p => p.Sku)
                .Skip((page - 1) * size)
                .Limit(size)
                .ToListAsync();

            return (productos, totalRegistros);
        }

        public async Task CrearAsync(Producto producto)
        {
            await _productosCollection.InsertOneAsync(producto);
        }

        public async Task<bool> ActualizarAsync(string sku, Producto producto)
        {
            var filtro = Builders<Producto>.Filter.Eq(p => p.Sku, sku);

            var actualizacion = Builders<Producto>.Update
                .Set(p => p.Nombre, producto.Nombre)
                .Set(p => p.Descripcion, producto.Descripcion)
                .Set(p => p.Categoria, producto.Categoria)
                .Set(p => p.Precio, producto.Precio)
                .Set(p => p.Stock, producto.Stock)
                .Set(p => p.Activo, producto.Activo)
                .Set(p => p.FechaActualizacion, producto.FechaActualizacion);

            var resultado = await _productosCollection.UpdateOneAsync(filtro, actualizacion);
            return resultado.MatchedCount > 0;
        }

        public async Task<bool> EliminarAsync(string sku)
        {
            var filtro = Builders<Producto>.Filter.Eq(p => p.Sku, sku);
            var resultado = await _productosCollection.DeleteOneAsync(filtro);
            return resultado.DeletedCount > 0;
        }

        public async Task<bool> ExisteCategoriaAsync(string nombreCategoria)
        {
            var filtro = Builders<Categoria>.Filter.Eq(c => c.Nombre, nombreCategoria);
            var categoria = await _categoriasCollection.Find(filtro).FirstOrDefaultAsync();
            return categoria is not null;
        }
    }
}
