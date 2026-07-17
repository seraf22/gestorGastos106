using Microsoft.AspNetCore.Mvc;
using Casa106.Application.Abstractions;
using Casa106.Application.DTOs;
using Casa106.Domain.Entities;
using Casa106.Domain.Enumerations;

namespace Casa106.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosController : ControllerBase
{
    private readonly IMovimientoRepository _movimientoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IPropiedadRepository _propiedadRepository;

    public MovimientosController(
        IMovimientoRepository movimientoRepository,
        ICategoriaRepository categoriaRepository,
        IPropiedadRepository propiedadRepository)
    {
        _movimientoRepository = movimientoRepository;
        _categoriaRepository = categoriaRepository;
        _propiedadRepository = propiedadRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<MovimientoDto>>> GetMovimientos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var propiedad = await _propiedadRepository.GetActivaAsync(cancellationToken);
        if (propiedad == null)
            return NotFound("No hay propiedad activa");

        var (movimientos, total) = await _movimientoRepository.GetPagedAsync(
            propiedad.Id, page, pageSize, cancellationToken);

        var dtos = movimientos.Select(m => MapToDto(m)).ToList();

        return Ok(new PaginatedResponse<MovimientoDto>
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovimientoDto>> GetMovimiento(Guid id, CancellationToken cancellationToken)
    {
        var movimiento = await _movimientoRepository.GetByIdAsync(id, cancellationToken);
        if (movimiento == null)
            return NotFound();

        return Ok(MapToDto(movimiento));
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoDto>> CreateMovimiento(
        CreateMovimientoRequest request,
        CancellationToken cancellationToken)
    {
        var propiedad = await _propiedadRepository.GetActivaAsync(cancellationToken);
        if (propiedad == null)
            return BadRequest("No hay propiedad activa");

        var categoria = await _categoriaRepository.GetByIdAsync(Guid.Parse(request.CategoriaId), cancellationToken);
        if (categoria == null)
            return BadRequest("Categoría no encontrada");

        var movimiento = new Movimiento
        {
            Id = Guid.NewGuid(),
            PropiedadId = propiedad.Id,
            CategoriaId = categoria.Id,
            Tipo = Enum.Parse<TipoMovimiento>(request.Tipo),
            Estado = EstadoMovimiento.Confirmado,
            Origen = OrigenMovimiento.Manual,
            FechaMovimiento = DateTime.SpecifyKind(
                request.FechaMovimiento,
                DateTimeKind.Utc
            ),
            PeriodoDesde = request.PeriodoDesde,
            PeriodoHasta = request.PeriodoHasta,
            Monto = request.Monto,
            Descripcion = request.Descripcion,
            Proveedor = request.Proveedor,
            MetodoPago = request.MetodoPago,
            FechaCreacion = DateTime.UtcNow
        };

        await _movimientoRepository.AddAsync(movimiento, cancellationToken);

        return CreatedAtAction(nameof(GetMovimiento), new { id = movimiento.Id }, MapToDto(movimiento));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMovimiento(
        Guid id,
        CreateMovimientoRequest request,
        CancellationToken cancellationToken)
    {
        var movimiento = await _movimientoRepository.GetByIdAsync(id, cancellationToken);
        if (movimiento == null)
            return NotFound();

        var categoria = await _categoriaRepository.GetByIdAsync(Guid.Parse(request.CategoriaId), cancellationToken);
        if (categoria == null)
            return BadRequest("Categoría no encontrada");

        movimiento.CategoriaId = categoria.Id;
        movimiento.Tipo = Enum.Parse<TipoMovimiento>(request.Tipo);
        movimiento.FechaMovimiento = request.FechaMovimiento;
        movimiento.PeriodoDesde = request.PeriodoDesde;
        movimiento.PeriodoHasta = request.PeriodoHasta;
        movimiento.Monto = request.Monto;
        movimiento.Descripcion = request.Descripcion;
        movimiento.Proveedor = request.Proveedor;
        movimiento.MetodoPago = request.MetodoPago;
        movimiento.FechaActualizacion = DateTime.UtcNow;

        await _movimientoRepository.UpdateAsync(movimiento, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMovimiento(Guid id, CancellationToken cancellationToken)
    {
        var movimiento = await _movimientoRepository.GetByIdAsync(id, cancellationToken);
        if (movimiento == null)
            return NotFound();

        await _movimientoRepository.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    private static MovimientoDto MapToDto(Movimiento m) => new()
    {
        Id = m.Id,
        PropiedadId = m.PropiedadId,
        CategoriaId = m.CategoriaId,
        Tipo = m.Tipo.ToString(),
        Estado = m.Estado.ToString(),
        Origen = m.Origen.ToString(),
        FechaMovimiento = m.FechaMovimiento,
        PeriodoDesde = m.PeriodoDesde,
        PeriodoHasta = m.PeriodoHasta,
        Monto = m.Monto,
        Descripcion = m.Descripcion,
        Proveedor = m.Proveedor,
        MetodoPago = m.MetodoPago,
        DocumentoId = m.DocumentoId,
        FechaCreacion = m.FechaCreacion,
        FechaActualizacion = m.FechaActualizacion,
        CategoriaNombre = m.Categoria?.Nombre
    };
}
