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
    public async Task<IActionResult> GetAll() {
        var query = _db.Parts.AsNoTracking().Include(x => x.Vendor).AsQueryable();
        if (!User.IsInRole("Admin")) query = query.Where(p => p.IsActive);
        return Ok(ApiResponse<object>.Ok(await query.ToListAsync()));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        var p = await _db.Parts.AsNoTracking().Include(x => x.Vendor).FirstOrDefaultAsync(x => x.Id == id);
        return p == null ? NotFound(ApiResponse<object>.Fail("Part not found.")) : Ok(ApiResponse<object>.Ok(p));
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLowStock() =>
        Ok(ApiResponse<object>.Ok(await _db.Parts.AsNoTracking().Include(p => p.Vendor)
            .Where(p => p.IsActive && p.StockQuantity <= p.ReorderLevel)
            .Select(p => new { p.Id, p.PartName, p.PartCode, p.Category, p.StockQuantity, p.ReorderLevel, p.UnitPrice, VendorName = p.Vendor != null ? p.Vendor.VendorName : "N/A" })
            .ToListAsync()));

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreatePartDto dto) {
        var duplicate = await _db.Parts.AnyAsync(p => p.PartCode == dto.PartCode);
        if (duplicate) return BadRequest(ApiResponse<object>.Fail("Part code already exists."));

        if (dto.VendorId.HasValue) {
            var vendorExists = await _db.Vendors.AsNoTracking().AnyAsync(v => v.Id == dto.VendorId.Value && v.IsActive);
            if (!vendorExists) return BadRequest(ApiResponse<object>.Fail("Selected vendor is not valid or inactive."));
        }

        var part = new Part {
            PartName = dto.PartName, PartCode = dto.PartCode, Category = dto.Category,
            Description = dto.Description ?? string.Empty,
            UnitPrice = dto.UnitPrice, StockQuantity = dto.StockQuantity, ReorderLevel = dto.ReorderLevel,
            VendorId = dto.VendorId,
            IsActive = dto.IsActive
        };
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = part.Id }, ApiResponse<object>.Ok(new {
            part.Id, part.PartName, part.PartCode, part.Category, part.Description,
            part.UnitPrice, part.StockQuantity, part.ReorderLevel, part.VendorId, part.IsActive
        }));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, UpdatePartDto dto) {
        var part = await _db.Parts.FindAsync(id);
        if (part == null) return NotFound(ApiResponse<object>.Fail("Part not found."));

        var duplicate = await _db.Parts.AnyAsync(p => p.PartCode == dto.PartCode && p.Id != id);
        if (duplicate) return BadRequest(ApiResponse<object>.Fail("Part code already exists."));

        if (dto.VendorId.HasValue) {
            var vendorExists = await _db.Vendors.AsNoTracking().AnyAsync(v => v.Id == dto.VendorId.Value && v.IsActive);
            if (!vendorExists) return BadRequest(ApiResponse<object>.Fail("Selected vendor is not valid or inactive."));
        }

        part.PartName = dto.PartName; part.PartCode = dto.PartCode;
        part.Category = dto.Category;
        part.Description = dto.Description ?? string.Empty;
        part.UnitPrice = dto.UnitPrice;
        part.StockQuantity = dto.StockQuantity;
        part.ReorderLevel = dto.ReorderLevel;
        part.VendorId = dto.VendorId;
        part.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<object>.Ok(new {
            part.Id, part.PartName, part.PartCode, part.Category, part.Description,
            part.UnitPrice, part.StockQuantity, part.ReorderLevel, part.VendorId, part.IsActive
        }));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id) {
        var part = await _db.Parts.FindAsync(id);
        if (part == null) return NotFound(ApiResponse<object>.Fail("Part not found."));
        part.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
