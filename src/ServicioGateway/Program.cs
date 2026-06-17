var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Registra YARP leyendo la sección "ReverseProxy" del appsettings.json
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// HttpClient para que el Gateway pueda llamar a los microservicios
// directamente en el endpoint de agregación
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware de logging: evidencia de observabilidad del Gateway
app.Use(async (context, next) =>
{
    var metodo = context.Request.Method;
    var ruta = context.Request.Path;
    Console.WriteLine($"[Gateway] {DateTime.Now:HH:mm:ss} -> Solicitud recibida: {metodo} {ruta}");

    await next();

    Console.WriteLine($"[Gateway] {DateTime.Now:HH:mm:ss} -> Respuesta enviada: {context.Response.StatusCode} para {ruta}");
});

app.UseHttpsRedirection();

// ---------- ENDPOINT DE AGREGACIÓN ----------
// Combina datos de OrdenCompra y Catálogo en una sola respuesta.
// Va ANTES de MapReverseProxy() para que tenga prioridad sobre las rutas genéricas.
app.MapGet("/api/resumen-orden/{id}", async (string id, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();

    // 1) Traer la orden desde MicroServicioOrdenCompra
    var urlOrden = $"https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioOrdenCompra/api/ordenes/{id}";
    var respuestaOrden = await client.GetAsync(urlOrden);

    if (!respuestaOrden.IsSuccessStatusCode)
    {
        return Results.NotFound(new { mensaje = "Orden no encontrada" });
    }

    using var ordenJson = System.Text.Json.JsonDocument.Parse(
        await respuestaOrden.Content.ReadAsStringAsync());

    Console.WriteLine($"[Gateway] {DateTime.Now:HH:mm:ss} -> Agregación: orden {id} obtenida de OrdenCompra");

    // 2) Por cada item de la orden, traer el detalle actual del producto desde Catálogo
    var detalleProductos = new List<object>();

    if (ordenJson.RootElement.TryGetProperty("items", out var items) &&
        items.ValueKind == System.Text.Json.JsonValueKind.Array)
    {
        foreach (var item in items.EnumerateArray())
        {
            if (item.TryGetProperty("productoId", out var productoIdProp))
            {
                var productoId = productoIdProp.GetString();
                var urlProducto = $"https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioCatalogo/api/productos/{productoId}";
                var respuestaProducto = await client.GetAsync(urlProducto);

                if (respuestaProducto.IsSuccessStatusCode)
                {
                    var productoTexto = await respuestaProducto.Content.ReadAsStringAsync();
                    detalleProductos.Add(System.Text.Json.JsonSerializer.Deserialize<object>(productoTexto)!);

                    Console.WriteLine($"[Gateway] {DateTime.Now:HH:mm:ss} -> Agregación: producto {productoId} obtenido de Catalogo");
                }
            }
        }
    }

    // 3) Combinar ambas respuestas en un solo objeto
    var resumen = new
    {
        orden = System.Text.Json.JsonSerializer.Deserialize<object>(ordenJson.RootElement.GetRawText()),
        productosActuales = detalleProductos,
        consultadoPorGateway = true,
        fecha = DateTime.Now
    };

    return Results.Ok(resumen);
});

// Activa YARP: todo lo que no matchee con una ruta de arriba, pasa por aquí
app.MapReverseProxy();

app.Run();