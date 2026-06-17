using MicroServicioOrdenCompra.Models;
using System.Text.Json;

namespace MicroServicioOrdenCompra.Services;

public interface IProductoService
{
    Task<ProductoDto?> ObtenerProductoAsync(string productoId);
}

public class ProductoService : IProductoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductoService> _logger;
    private readonly IConfiguration _config;

    public ProductoService(HttpClient httpClient, ILogger<ProductoService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;

        var baseUrl = _config["ServicioProductos:BaseUrl"] ?? "http://localhost:5001";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<ProductoDto?> ObtenerProductoAsync(string productoId)
    {
        try
        {
            _logger.LogInformation("[ProductoService] Consultando producto {ProductoId}", productoId);

            var response = await _httpClient.GetAsync($"/api/productos/{productoId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("[ProductoService] Producto {ProductoId} no encontrado", productoId);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<ProductoDto>(json, options);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "[ProductoService] Error al conectar con servicio de productos");
            return null;
        }
    }
}