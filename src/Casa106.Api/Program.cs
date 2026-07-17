using Casa106.Infrastructure.Persistence;
using Casa106.Infrastructure.Storage;
using Casa106.Infrastructure.FinancialAI;
using Casa106.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (PostgreSQL / Aiven)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Falta 'ConnectionStrings:DefaultConnection' para PostgreSQL.");
}

builder.Services.AddDbContext<Casa106DbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped<IMovimientoRepository, MovimientoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IPropiedadRepository, PropiedadRepository>();
builder.Services.AddScoped<IDocumentoRepository, DocumentoRepository>();

// Infrastructure Services
builder.Services.AddScoped<IFinancialDocumentAnalyzer, FakeFinancialDocumentAnalyzer>();

var cloudinaryConfig = builder.Configuration.GetSection("Cloudinary");
if (!string.IsNullOrWhiteSpace(cloudinaryConfig["CloudName"]) &&
    !string.IsNullOrWhiteSpace(cloudinaryConfig["ApiKey"]) &&
    !string.IsNullOrWhiteSpace(cloudinaryConfig["ApiSecret"]))
{
    builder.Services.AddScoped<IDocumentStorage, CloudinaryDocumentStorage>();
}
else
{
    builder.Services.AddScoped<IDocumentStorage>(sp => new LocalDocumentStorage(
        Path.Combine(builder.Environment.ContentRootPath, "uploads")));
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Inicializar y seed de base de datos
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Casa106DbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedDatabase(db);
}

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task SeedDatabase(Casa106DbContext context)
{
    if (await context.Propiedades.AnyAsync())
        return;

    var propiedad = new Casa106.Domain.Entities.Propiedad
    {
        Id = Guid.NewGuid(),
        Nombre = "Casa 106",
        Direccion = "Pucón",
        Unidad = "106",
        Activa = true,
        FechaCreacion = DateTime.UtcNow
    };

    var categorias = new[]
    {
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Arriendo Airbnb", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Ingreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 1, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Arriendo directo", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Ingreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 2, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Reembolso", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Ingreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Otro, Orden = 3, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Devolución", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Ingreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Otro, Orden = 4, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Otros ingresos", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Ingreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Otro, Orden = 5, Activa = true },

        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Comisión administrador", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 1, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Comisión Airbnb", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 2, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Aseo", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 3, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Gastos comunes", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 4, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Electricidad", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 5, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Agua", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 6, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Internet", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 7, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Gas", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 8, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Pellet", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 9, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Mantenimiento", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 10, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Reparaciones", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Operacional, Orden = 11, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Muebles y equipamiento", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Inversion, Orden = 12, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Contribuciones", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Impuesto, Orden = 13, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Seguros", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Financiero, Orden = 14, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Dividendo", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Financiero, Orden = 15, Activa = true },
        new Casa106.Domain.Entities.Categoria { Id = Guid.NewGuid(), Nombre = "Otros egresos", TipoMovimiento = Casa106.Domain.Enumerations.TipoMovimiento.Egreso, Grupo = Casa106.Domain.Enumerations.GrupoCategoria.Otro, Orden = 16, Activa = true }
    };

    context.Propiedades.Add(propiedad);
    context.Categorias.AddRange(categorias);

    await context.SaveChangesAsync();
}
