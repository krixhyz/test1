using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class VendorsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public VendorsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(ApiResponse<object>.Ok(await _db.Vendors.AsNoTracking().ToListAsync()));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        var v = await _db.Vendors.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return v == null ? NotFound(ApiResponse<object>.Fail("Vendor not found.")) : Ok(ApiResponse<object>.Ok(v));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVendorDto dto) {
        var vendor = new Vendor { VendorName = dto.VendorName, Phone = dto.Phone, Email = dto.Email, Address = dto.Address, ContactPerson = dto.ContactPerson };
        _db.Vendors.Add(vendor);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = vendor.Id }, ApiResponse<Vendor>.Ok(vendor));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateVendorDto dto) {
        var vendor = await _db.Vendors.FindAsync(id);
        if (vendor == null) return NotFound(ApiResponse<object>.Fail("Vendor not found."));
        vendor.VendorName = dto.VendorName; vendor.Phone = dto.Phone;
        vendor.Email = dto.Email; vendor.Address = dto.Address;
        vendor.ContactPerson = dto.ContactPerson; vendor.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Vendor>.Ok(vendor));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id) {
        var vendor = await _db.Vendors.FindAsync(id);
        if (vendor == null) return NotFound(ApiResponse<object>.Fail("Vendor not found."));
        _db.Vendors.Remove(vendor);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}