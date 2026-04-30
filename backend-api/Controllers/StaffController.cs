using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Common;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase {
    private readonly UserManager<ApplicationUser> _userManager;
    public StaffController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    [HttpGet]
    public async Task<IActionResult> GetAll() {
        var staff = await _userManager.GetUsersInRoleAsync("Staff");
        var result = staff.Select(s => new { s.Id, s.Email, s.FullName, s.PhoneNumber, s.Position, s.IsActive, s.CreatedAt });
        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(ApiResponse<object>.Fail("Staff not found."));
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Staff")) return NotFound(ApiResponse<object>.Fail("Staff not found."));
        return Ok(ApiResponse<object>.Ok(new { user.Id, user.Email, user.FullName, user.PhoneNumber, user.Position, user.IsActive, user.CreatedAt }));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStaffDto dto) {
        var user = new ApplicationUser {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.Phone,
            Position = dto.Position ?? string.Empty,
            IsActive = dto.IsActive
        };
        var res = await _userManager.CreateAsync(user, dto.Password);
        if (!res.Succeeded) return BadRequest(ApiResponse<object>.Fail(string.Join(", ", res.Errors.Select(e => e.Description))));
        await _userManager.AddToRoleAsync(user, "Staff");
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ApiResponse<object>.Ok(new { user.Id, user.Email, user.FullName, user.Position, user.IsActive }));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateStaffDto dto) {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(ApiResponse<object>.Fail("Staff not found."));
        user.FullName = dto.FullName;
        user.PhoneNumber = dto.Phone;
        user.Position = dto.Position ?? string.Empty;
        user.IsActive = dto.IsActive;
        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded) return BadRequest(ApiResponse<object>.Fail(string.Join(", ", res.Errors.Select(e => e.Description))));
        return Ok(ApiResponse<object>.Ok(new { user.Id, user.Email, user.FullName, user.PhoneNumber, user.Position, user.IsActive }));
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id) {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(ApiResponse<object>.Fail("Staff not found."));
        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        return Ok(ApiResponse<object>.Ok(new { user.Id, user.IsActive }));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id) {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound(ApiResponse<object>.Fail("Staff not found."));
        await _userManager.DeleteAsync(user);
        return NoContent();
    }
}
