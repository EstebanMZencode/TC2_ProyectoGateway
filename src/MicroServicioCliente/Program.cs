using MicroServicioCliente.Configuration;
using MicroServicioCliente.Services;

var builder = WebApplication.CreateBuilder(args);

//  Configuración de MongoDB 
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

//  Registro de servicios 
builder.Services.AddSingleton<ClienteService>();

//  Controladores 
builder.Services.AddControllers();

//  Swagger / OpenAPI 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "MicroServicioCliente API",
        Version = "v1",
        Description = "API REST para gestión de clientes – TC2 Proyecto Gateway"
    });

    // Incluir comentarios XML de los controladores
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── CORS (necesario para que el Gateway pueda consumirlo) ─────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// PathBase para Plesk (la URL de producción incluye /MicroServicioCliente) 
var pathBase = builder.Configuration.GetValue<string>("PathBase") ?? "/MicroServicioCliente";
builder.WebHost.UseUrls("http://0.0.0.0:5002");

var app = builder.Build();

app.UsePathBase(pathBase);
app.UseRouting();

// Swagger siempre activo (incluso en producción, como el resto del equipo) 
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", "MicroServicioCliente v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("GatewayPolicy");

app.MapControllers();

// Endpoint de health check para el Gateway
app.MapGet("/health", () => Results.Ok(new
{
    servicio = "MicroServicioCliente",
    estado = "activo",
    timestamp = DateTime.UtcNow
}));

app.Logger.LogInformation("MicroServicioCliente iniciado en puerto 5002 con PathBase: {PathBase}", pathBase);

app.Run();