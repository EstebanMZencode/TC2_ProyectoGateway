using MicroServicioCatalogo;
using MicroServicioCatalogo.Repository;
using MicroServicioCatalogo.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

var app = builder.Build();

//app.UsePathBase("https://tiusr23pl.cuc-carrera-ti.ac.cr/MicroServicioCatalogo");
app.UsePathBase("/MicroServicioCatalogo");


app.UseHttpsRedirection();

app.MapProductoEndpoints();

app.Run();
