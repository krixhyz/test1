using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize]
public class PartsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public PartsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Parts.AsNoTracking().Include(x => x.Vendor).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        var p = await _db.Parts.AsNoTracking().Include(x => x.Vendor).FirstOrDefaultAsync(x => x.Id == id);
        return p == null ? NotFound(ApiResponse<object>.Fail("Part not found.")) : Ok(ApiResponse<object>.Ok(p));
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLowStock() =>
        Ok(await _db.Parts.AsNoTracking().Where(p => p.StockQuantity <= p.ReorderLevel).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreatePartDto dto) {
        var part = new Part {
            PartName = dto.PartName, PartCode = dto.PartCode, Category = dto.Category,
            UnitPrice = dto.UnitPrice, StockQuantity = dto.StockQuantity, ReorderLevel = dto.ReorderLevel,
            VendorId = dto.VendorId ?? Guid.Empty
        };
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = part.Id }, ApiResponse<Part>.Ok(part));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdatePartDto dto) {
        var part = await _db.Parts.FindAsync(id);
        if (part == null) return NotFound(ApiResponse<object>.Fail("Part not found."));
        part.PartName = dto.PartName; part.PartCode = dto.PartCode;
        part.Category = dto.Category; part.UnitPrice = dto.UnitPrice;
        part.ReorderLevel = dto.ReorderLevel;
        if (dto.VendorId.HasValue) part.VendorId = dto.VendorId.Value;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Part>.Ok(part));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id) {
        var part = await _db.Parts.FindAsync(id);
        if (part == null) return NotFound(ApiResponse<object>.Fail("Part not found."));
        _db.Parts.Remove(part);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}