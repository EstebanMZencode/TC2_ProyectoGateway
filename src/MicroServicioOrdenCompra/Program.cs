using MicroServicioOrdenCompra.Config;
using MicroServicioOrdenCompra.Repositories;
using MicroServicioOrdenCompra.Services;

var builder = WebApplication.CreateBuilder(args);

// ── MongoDB ────────────────────────────────────────────────────────────────
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

// ── Repositorios y Servicios ───────────────────────────────────────────────
builder.Services.AddSingleton<IOrdenRepository, OrdenRepository>();
builder.Services.AddHttpClient<IProductoService, ProductoService>();
builder.Services.AddScoped<IOrdenService, OrdenService>();

// ── Controllers ────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// ── CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("PermitirTodo");
app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("════════════════════════════════════════");
app.Logger.LogInformation("  MicroServicioOrdenCompra iniciado");
app.Logger.LogInformation("  Puerto: http://localhost:5002");
app.Logger.LogInformation("════════════════════════════════════════");

app.Run();